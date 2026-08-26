using UnityEngine;
using thisEState = LegStateMachine.ELegState; // shorthands
using EEnviroment = EnviromentInteractionStateMachine.EEnviromentInteractionState;

public class LegAirAproachState : LegState
{
    private float _speed;
    private Vector3 _transDisplace;
    public LegAirAproachState(LegContext lContext, thisEState estate) : base(lContext, estate)
    {

    }

    public override void EnterState()
    {
        SetFDetectorEnabled(true);
    }
    public override void ExitState()
    {
        SetFDetectorEnabled(false);
    }
    public override void UpdateState()
    {
        //find and set next IK target
        FindPerLerpPos();
    }
    public override void LateUpdateState()
    {
        CalcNextIkAirPosition();
        SetIkTarget(LContext.StridePos, LContext.StrideRotation); //#
        HoldIkTarget();
    }
    public override thisEState GetNextState()
    {
        //reset to walk
        if (Co.Eism.CurrentStateKey == EEnviroment.Walk || Co.Eism.CurrentStateKey == EEnviroment.Wall)
        {
            AirToWalkExitPrep();
            if (!LContext.StrideInAir)
            {
                AirToWalkExitChecks();
                return thisEState.Search;
            }
        }

        //leave near obstacle to air
        if (!Co.Tracking)
        {
            return thisEState.AirSearch;
        }
        return StateKey;
    }

    private void FindPerLerpPos()
    {
        // strength Lerp between positions
        Vector3 OGLocalPos = Co.RootTransform.TransformPoint(LContext.OGLocalAirPos);
        Vector3 trackPoint = Co.TrackPoint;
        Vector3 extendDir = trackPoint - LContext.ThisLegTransform.position;
        Vector3 tipDir = trackPoint - LContext.ThisIkConstraint.data.tip.position;
        Vector3 projectVel = Vector3.Project(Co.Rb.linearVelocity, tipDir);
        float time, timeRatio;
        if (Vector3.Dot(projectVel.normalized, tipDir.normalized) > 0) // negative when leaving
        {
            _speed = projectVel.magnitude;
            time = tipDir.magnitude / _speed;
            time = Mathf.Clamp(time, 0, Co.TimeCap);
            timeRatio = time / Co.TimeCap; 
        }
        else
        {
            timeRatio = 1;
        }

        Vector3 extendPos = LContext.ThisLegTransform.position + LContext.LegLength * extendDir.normalized;
        Vector3 worldDesiPos = Vector3.Lerp(OGLocalPos, extendPos, Co.AirDisLegExtendCurve.Evaluate(timeRatio)); // max extension is LegLength

        _transDisplace = worldDesiPos - LContext.ThisLegTransform.position; // desi pos should orbit root and be reppeled by it to prevent extreme movements
        float reppelMag = Co.AirHipMaxRepelDis / (1 + Mathf.Pow(Co.AirHipRepelMult * _transDisplace.magnitude, 2)); // calc reppel based on distance : a/(1 + (bx)^2)
        Vector3 reppel = _transDisplace.normalized * reppelMag;
        worldDesiPos += reppel;

        Co.LocalDesiStore[LContext.Side] = Co.RootTransform.InverseTransformPoint(worldDesiPos);
    }

    private void CalcNextIkAirPosition()
    {
        //Double foot reppel
        Vector3 localDesiPos = Co.LocalDesiStore[LContext.Side];
        Vector3 localOtherPos = Co.LocalDesiStore[LContext.OtherSide];
        Vector3 reppel = localDesiPos - localOtherPos; // tip pos orbit each over
        float reppelMag = Co.AirTipMaxRepelDis / (1 + Mathf.Pow(Co.AirTipRepelMult * reppel.magnitude, 2)); // calc reppel based on distance : a/(1 + (bx)^2)
        reppel = reppel.normalized * reppelMag;
        localDesiPos += reppel;

        // Lerp from current to Desi
        float tempDif = (LContext.LocalAirPos - localDesiPos).magnitude;
        tempDif = Mathf.Clamp(tempDif / LContext.LegLength, 0, 1);
        float eval = Co.AirPosLerpCurve.Evaluate(tempDif); // faster at greater changes
        eval *= 1 + _speed / Co.PosLerpSpeedMod; // faster when high velocity

        //Vector3 overPos = LContext.OvershootPositionCalc(LContext.LocalAirPos, localDesiPos, eval, Time.deltaTime);
        Vector3 applyLPos = Vector3.Lerp(LContext.LocalAirPos, localDesiPos, Time.deltaTime * eval);
        Vector3 applyWPos = Co.RootTransform.TransformPoint(applyLPos);

        //raycast prevent clipping through walls
        RaycastHit tempHit;
        Vector3 tempDis = applyWPos - LContext.ThisLegTransform.position;
        if (Physics.Raycast(LContext.ThisLegTransform.position, tempDis, out tempHit, tempDis.magnitude + Co.PlaceOffsetDis, Co.GroundLayer))
        {
            applyWPos = tempHit.point + tempHit.normal * Co.PlaceOffsetDis;// unclip and add PlaceOffsetDis
            applyLPos = Co.RootTransform.InverseTransformPoint(applyWPos);
        }

        LContext.StridePos = applyWPos;
        //save next
        LContext.LocalAirPos = applyLPos;
        
        //find rotation
        Vector3 netFwd = LContext.ThisIkConstraint.data.mid.forward; // #
        Quaternion desiRot = Quaternion.FromToRotation(Vector3.up, netFwd) * Quaternion.FromToRotation(Vector3.forward, Vector3.ProjectOnPlane(Co.RootTransform.forward, Vector3.up)); // foot perpendicular to shin
        float angleDif = Vector3.Angle(netFwd, LContext.StrideRotation * Vector3.forward) / 180;
        LContext.StrideRotation = Quaternion.Lerp(LContext.StrideRotation, desiRot, Time.deltaTime * Mathf.Pow(angleDif, 1 / Co.AirRotLerpMult) * Co.AirRotLerpMult);
    }
}
