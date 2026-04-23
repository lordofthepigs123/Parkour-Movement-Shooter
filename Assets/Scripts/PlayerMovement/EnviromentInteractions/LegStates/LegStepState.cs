using UnityEngine;
using thisEState = LegStateMachine.ELegState; // shorthand

public class LegStepState : LegState
{
    public LegStepState(LegContext lContext, thisEState estate) : base(lContext, estate)
    {

    }

    public override void EnterState()
    {
        LContext.ThisOppositeInvalidState = true;
    }
    public override void ExitState()
    {
        LContext.ThisOppositeInvalidState = false;
    }
    public override void UpdateState()
    {
        LContext.FindLegNormal();//#more robust
        //other leg can't step until this toe past center
        LContext.ThisOppositeInvalidState = LContext.DistanceFromCenterFlat() < 0;

        FindIkStepPosition();
        SetIkTarget(LContext.StepHit.point, LContext.StepHit.normal); //#
    }
    public override thisEState GetNextState()
    {
        bool inHitProximity = LContext.ActivePointDistance() < Co.MinActivePointDistance;
        bool hitStepPointValid = LContext.StepHit.collider != null;

        if (inHitProximity && hitStepPointValid)
        {
            //home in on contact position, frozen
            LContext.FinalDestination = LContext.StepHit;
            return thisEState.Reset;
        }
        return StateKey;
    }
}
