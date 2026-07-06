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
        CalculateStride();

        //active estimate of final landing step point
        FindIkStepPosition();

        //set IK target
        HoldIkTarget();
    }
    public override thisEState GetNextState()
    {
        bool strideDisPassed = LContext.DistanceFromCenterFlat(LContext.ThisIkConstraint.data.tip.position) < -LContext.BackStride;
        bool hitStepPointValid = LContext.StepCol != null;
        bool otherFootValidPosition = !LContext.ThisOppositeInvalidState[LContext.OtherSide];
        bool conditions = strideDisPassed && hitStepPointValid && otherFootValidPosition;
        bool altConditions = (LContext.Side == EnviromentInteractionContext.EBodySide.RIGHT) && strideDisPassed && hitStepPointValid && !otherFootValidPosition;

        //round check
        if (conditions)
        {
            Debug.Log("Search -> Step1");
            return thisEState.Step;
        }
        else if (altConditions)
        {// if both invalid after loop Right leg priority
            Debug.Log("Search -> Step2");
            return thisEState.Step;
        }
        else
        {
            LContext.ThisOppositeInvalidState[LContext.Side] = false; //redund
        }

        return StateKey;
    }        
    
} 