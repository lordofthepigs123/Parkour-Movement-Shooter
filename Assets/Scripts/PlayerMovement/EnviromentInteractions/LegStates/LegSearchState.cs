using UnityEngine;

using thisEState = LegStateMachine.ELegState; // shorthand

public class LegSearchState : LegState
{
    public LegSearchState(LegContext lContext, thisEState estate) : base(lContext, estate)
    {

    }

    public override void EnterState(){}
    public override void ExitState(){}
    public override void UpdateState()
    {
        LContext.FindLegNormal();//#
        FindIkStepPosition();
    }
    public override thisEState GetNextState()
    {
        Debug.DrawRay(LContext.StepHit.point, LContext.StepHit.normal, Color.red, 1);

        bool strideDisPassed = StrideDisPassed();
        bool hitStepPointValid = LContext.StepHit.collider != null;
        bool otherFootValidPosition = !LContext.ThisInvalidState;
        //Debug.Log(strideDisPassed +" "+ hitStepPointValid +" "+ otherFootValidPosition);
        if (strideDisPassed && hitStepPointValid && otherFootValidPosition)
        {
            return thisEState.Step;
        }
        return StateKey;
    }
    
} 