using UnityEngine;
using thisEState = LegStateMachine.ELegState; // shorthand

public class LegAirSearchState : LegState
{
    public LegAirSearchState(LegContext lContext, thisEState estate) : base(lContext, estate)
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
    }
    public override thisEState GetNextState()
    {
        if (LContext.ThisLegNormal != Vector3.zero)
        {
            SetIkTarget(LContext.ThisLegPoint, LContext.ThisLegNormal);
            Debug.Log("airSearch -> reset");
            return thisEState.Reset;
        }
        return StateKey;
    }
}
