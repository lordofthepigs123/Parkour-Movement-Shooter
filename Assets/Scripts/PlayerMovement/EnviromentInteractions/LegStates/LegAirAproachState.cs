using UnityEngine;
using thisEState = LegStateMachine.ELegState; // shorthands
using EEnviroment = EnviromentInteractionStateMachine.EEnviromentInteractionState;

public class LegAirAproachState : LegState
{
    public LegAirAproachState(LegContext lContext, thisEState estate) : base(lContext, estate)
    {

    }

    public override void EnterState()
    {

    }
    public override void ExitState()
    {
    
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
            LContext.FindLegNormal();//#
            LContext.StridePos = LContext.ThisLegPoint + LContext.ThisLegNormal * Co.PlaceOffsetDis;
            LContext.StrideRotation = Quaternion.FromToRotation(Vector3.up,LContext.ThisLegNormal) * Quaternion.FromToRotation(Vector3.forward, Vector3.ProjectOnPlane(Co.RootTransform.forward, Vector3.up));
            SetIkTarget(LContext.StridePos, LContext.StrideRotation);
            //Debug.Log("AirSearch -> Search");
            return thisEState.Search;
        }
        return StateKey;
    }

    private void FindNextIkAirPosition()
    {
        
        float tempDif = (LContext.LocalAirPos - LContext.DesiLocalAirPos).magnitude;
        tempDif = Mathf.Clamp(tempDif / LContext.LegLength, 0, 1);
        float eval = Co.AirPosLerpCurve.Evaluate(tempDif);
        Vector3 applyLPos = Vector3.Lerp(LContext.LocalAirPos, LContext.DesiLocalAirPos, Time.deltaTime * eval);
        LContext.StridePos = Co.RootTransform.TransformPoint(applyLPos);
        //save next
        LContext.LocalAirPos = applyLPos;
        
        //find rotation
        Quaternion desiRot = Quaternion.FromToRotation(Vector3.up, LContext.ThisIkConstraint.data.mid.forward) * Quaternion.FromToRotation(Vector3.forward, Vector3.ProjectOnPlane(Co.RootTransform.forward, Vector3.up)); // foot perpendicular to shin
        float angleDif = Vector3.Angle(LContext.ThisIkConstraint.data.mid.forward, LContext.StrideRotation * Vector3.forward) / 180;
        LContext.StrideRotation = Quaternion.Lerp(LContext.StrideRotation, desiRot, Time.deltaTime * Mathf.Pow(angleDif, 1 / Co.AirRotLerpMult) * Co.AirRotLerpMult);
    }
}
