using UnityEngine;
using thisEState = LegStateMachine.ELegState; // shorthands
using EEnviroment = EnviromentInteractionStateMachine.EEnviromentInteractionState;

public class LegAirJumpState : LegState
{
    public LegAirJumpState(LegContext lContext, thisEState estate) : base(lContext, estate)
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
        LContext.FindLegNormal();//#

        //find and set next IK target
        FindNextIkAirPosition();
        SetIkTarget(LContext.StridePos, LContext.StrideRotation); //#
        HoldIkTarget();
    }
    public override void LateUpdateState(){}
    public override thisEState GetNextState()
    {
        // transition to normal air state 
        float tempDis = (LContext.StridePos - LContext.ThisLegTransform.position).magnitude;
        if (tempDis > LContext.LegLength + 0.2f) // check when push off leg is overstreched
        {
            return thisEState.AirSearch;
        }

        //reset to walk
        if (Co.Eism.CurrentStateKey == EEnviroment.Walk)
        {
            LContext.StridePos = LContext.ThisLegPoint + LContext.ThisLegNormal * Co.PlaceOffsetDis;
            LContext.StrideRotation = Quaternion.FromToRotation(Vector3.up,LContext.ThisLegNormal) * Quaternion.FromToRotation(Vector3.forward, Vector3.ProjectOnPlane(Co.RootTransform.forward, Vector3.up));
            SetIkTarget(LContext.StridePos, LContext.StrideRotation);
            //Debug.Log("AirJump -> Search");
            return thisEState.Search;
        }
        return StateKey;
    }

    private void FindNextIkAirPosition()
    {
        Vector3 desiPos = LContext.StepPos;
        LContext.StridePos = Vector3.Lerp(LContext.StridePos, desiPos, Time.deltaTime * 5);
        LContext.StrideRotation = Quaternion.FromToRotation(Vector3.up, Co.SmoothPlayerNormal) * Quaternion.FromToRotation(Vector3.forward, Vector3.ProjectOnPlane(Co.RootTransform.forward, Vector3.up));
    }
}