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
            SetIkTarget(LContext.ThisLegPoint, Quaternion.FromToRotation(Vector3.up,LContext.ThisLegNormal) * Quaternion.FromToRotation(Vector3.forward, Vector3.ProjectOnPlane(Co.RootTransform.forward, Vector3.up)));
            Debug.Log("AirSearch -> Search");
            return thisEState.Search;
        }
        return StateKey;
    }
}
