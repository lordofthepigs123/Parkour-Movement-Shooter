using UnityEngine;

using thisEState = LegStateMachine.ELegState; // shorthand

public class LegResetState : LegState
{
    float _contactTime = 0;
    public LegResetState(LegContext lContext, thisEState estate) : base(lContext, estate)
    {
        LegContext LContext = lContext;
    }

    public override void EnterState(){}
    public override void ExitState(){}
    public override void UpdateState(){}
    public override thisEState GetNextState()
    {
        float resetTime = Co.ResetDur * Mathf.Pow(Co.ResetDurMod, Velocity.magnitude); // t / (1+k)^x
        if (_contactTime >= resetTime)
        return LegStateMachine.ELegState.Search;
        //return StateKey;
    }
    public override void OnTriggerEnter(Collider other){}
    public override void OnTriggerStay(Collider other){}
    public override void OnTriggerExit(Collider other){}
}