using UnityEngine;
using thisEState = LegStateMachine.ELegState; // shorthands
using EEnviroment = EnviromentInteractionStateMachine.EEnviromentInteractionState;

public class LegAirJumpState : LegState
{
    private float tipRatio;
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
        LContext.FindFootNormal(-Co.RootTransform.up);

        //find and set next IK target
        FindNextIkAirPosition();
        SetIkTarget(LContext.StridePos, LContext.StrideRotation); //#
        HoldIkTarget();
    }
    public override void LateUpdateState(){}
    public override thisEState GetNextState()
    {
        // transition to normal air state 
        float tipDis = (LContext.StridePos - LContext.ThisLegTransform.position).magnitude;
        tipRatio = tipDis / (LContext.LegLength + 0.3f);
        if (tipRatio > 1) // check when push off leg is overstreched
        {
            return thisEState.AirSearch;
        }

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
        return StateKey;
    }

    private void FindNextIkAirPosition()
    {
        Vector3 desiPos = LContext.StepPos;
        Vector3 desiNormal = Vector3.Lerp(LContext.ThisLegNormal, LContext.ThisIkConstraint.data.mid.forward, tipRatio);
        desiPos += (1 - Vector3.Dot(LContext.ThisLegNormal,desiNormal)) * Co.FootLength * LContext.ThisLegNormal; // anti clip rise
        LContext.StridePos = Vector3.Lerp(LContext.StridePos, desiPos, Time.deltaTime * 10);

        LContext.StrideRotation = Quaternion.FromToRotation(Vector3.up, desiNormal) * Quaternion.FromToRotation(Vector3.forward, Vector3.ProjectOnPlane(Co.RootTransform.forward, Vector3.up));
    }
}