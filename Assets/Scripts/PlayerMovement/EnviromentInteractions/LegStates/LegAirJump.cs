using UnityEngine;
using thisEState = LegStateMachine.ELegState; // shorthands
using EEnviroment = EnviromentInteractionStateMachine.EEnviromentInteractionState;

public class LegAirJump : LegState
{    public LegAirJump(LegContext lContext, thisEState estate) : base(lContext, estate)
    {
        
    }

    public override void EnterState()
    {
        //decide front leg
        LContext.AirFrontLeg = (Co.LastStepDir == EnviromentInteractionContext.EStepDir.FORWARD && Co.LastStepSide == LContext.Side) || (Co.LastStepDir == EnviromentInteractionContext.EStepDir.BACKWARD && Co.LastStepSide != LContext.Side);
    }
    public override void ExitState()
    {

    }
    public override void UpdateState()
    {
        LContext.FindLegNormal();//#

        //find and set next IK target
        FindNextIkStridePosition();
        SetIkTarget(LContext.StridePos, LContext.StrideRotation); //#
        HoldIkTarget();
    }
    public override void LateUpdateState(){}
    public override thisEState GetNextState()
    {
        // transition to normal air state 

        //reset to walk
        if (Co.Eism.CurrentStateKey == EEnviroment.Walk)
        {
            SetIkTarget(LContext.ThisLegPoint, Quaternion.FromToRotation(Vector3.up,LContext.ThisLegNormal) * Quaternion.FromToRotation(Vector3.forward, Vector3.ProjectOnPlane(Co.RootTransform.forward, Vector3.up)));
            //Debug.Log("AirJump -> Search");
            return thisEState.Search;
        }
        return StateKey;
    }

    private void FindNextIkStridePosition()
    {
        //LContext.StridePos;
        //LContext.StrideRotation;
    }
}