using UnityEngine;

using thisEState = LegStateMachine.ELegState; // shorthand

public class LegResetState : LegState
{
    public LegResetState(LegContext lContext, thisEState estate) : base(lContext, estate)
    {
        LegContext LContext = lContext;
    }

    public override void EnterState(){}
    public override void ExitState(){}
    public override void UpdateState(){}
    public override thisEState GetNextState()
    {
        return LegStateMachine.ELegState.Search;
        //return StateKey;
    }
    public override void OnTriggerEnter(Collider other){}
    public override void OnTriggerStay(Collider other){}
    public override void OnTriggerExit(Collider other){}
}