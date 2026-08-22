using UnityEngine;
using thisEState = LegStateMachine.ELegState; // shorthands
using EEnviroment = EnviromentInteractionStateMachine.EEnviromentInteractionState;

public class LegAirSearchState : LegState
{
    private Vector3 _transDisplace;
    public LegAirSearchState(LegContext lContext, thisEState estate) : base(lContext, estate)
    {

    }

    public override void EnterState()
    {
        SetFDetectorEnabled(true); // enable detector for Tracking

        if (LContext.AirFrontLeg != LegContext.FrontLeg.UNSET)
            return;
        //decide front leg
        LContext.AirFrontLeg = (Co.LastStepDir == EnviromentInteractionContext.EStepDir.FORWARD && Co.LastStepSide == LContext.Side) 
        || (Co.LastStepDir == EnviromentInteractionContext.EStepDir.BACKWARD && Co.LastStepSide != LContext.Side) ?
        LegContext.FrontLeg.TRUE : LegContext.FrontLeg.FLASE;

        LContext.LocalAirPos = Co.RootTransform.InverseTransformPoint(LContext.StridePos); // Preset local position of target
        SetDesiLocalPos(); // save air reference local position 
    }
    public override void ExitState()
    {
        SetFDetectorEnabled(false);
    }
    public override void UpdateState()
    {
        //find and set next IK target
        FindNextIkAirPosition();
        SetIkTarget(LContext.StridePos, LContext.StrideRotation); //#
        HoldIkTarget();
    }
    public override void LateUpdateState(){}
    public override thisEState GetNextState()
    {
        //reset to walk
        if (Co.Eism.CurrentStateKey == EEnviroment.Walk)
        {
            AirToWalkExitPrep();
            if (!LContext.StrideInAir)
            {
                AirToWalkExitChecks();
                return thisEState.Search;
            }
        }

        //near obstacle aproach
        if (Co.Tracking)
        {
            return thisEState.AirAproach;
        }
        return StateKey;
    }

    private void FindNextIkAirPosition()
    {
        float speed = Co.Rb.linearVelocity.magnitude;
        float speedMult = 1 + speed / Co.PosLerpSpeedMod;
        float tempDif = (LContext.LocalAirPos - LContext.OGLocalAirPos).magnitude;
        tempDif = Mathf.Clamp(tempDif / LContext.LegLength, 0, 1);
        float eval = Co.AirPosLerpCurve.Evaluate(tempDif); // faster at greater changes
        eval *= speedMult;

        //Vector3 overPos = LContext.OvershootPositionCalc(LContext.LocalAirPos, LContext.OGLocalAirPos, eval, Time.deltaTime);
        Vector3 applyLPos = Vector3.Lerp(LContext.LocalAirPos, LContext.OGLocalAirPos, Time.deltaTime * eval);
        LContext.StridePos = Co.RootTransform.TransformPoint(applyLPos);
        //save next
        LContext.LocalAirPos = applyLPos;
        
        //find rotation
        //_transDisplace = Co.RootTransform.TransformPoint(LContext.OGLocalAirPos) - LContext.ThisLegTransform.position;
        Vector3 netFwd = LContext.ThisIkConstraint.data.mid.forward; // #
        Quaternion desiRot = Quaternion.FromToRotation(Vector3.up, netFwd) * Quaternion.FromToRotation(Vector3.forward, Vector3.ProjectOnPlane(Co.RootTransform.forward, Vector3.up)); // foot perpendicular to shin
        float angleDif = Vector3.Angle(netFwd, LContext.StrideRotation * Vector3.forward) / 180;
        LContext.StrideRotation = Quaternion.Lerp(LContext.StrideRotation, desiRot, Time.deltaTime * Mathf.Pow(angleDif, 1 / Co.AirRotLerpMult) * Co.AirRotLerpMult);
    }

    private void SetDesiLocalPos()
    {
        //find neutral position
        Vector3 desiPos;
        if (LContext.AirFrontLeg == LegContext.FrontLeg.TRUE) // apply which leg has frontal position after jump
        {
            desiPos = Co.FwdAirSearchReference.position;
            if (LContext.Side == EnviromentInteractionContext.EBodySide.LEFT)
                desiPos = BodySideReflect(desiPos);
        }
        else
        {
            desiPos = Co.BwdAirSearchReference.position;
            if (LContext.Side == EnviromentInteractionContext.EBodySide.RIGHT)
                desiPos = BodySideReflect(desiPos);
        }

        LContext.OGLocalAirPos = Co.RootTransform.InverseTransformPoint(desiPos); // make local
    }

    private Vector3 BodySideReflect(Vector3 pos)
    {
        Vector3 perpenDis = Vector3.Project(Co.RootTransform.position - pos, Co.RootTransform.right);
        return pos + perpenDis * 2;
    }
}
