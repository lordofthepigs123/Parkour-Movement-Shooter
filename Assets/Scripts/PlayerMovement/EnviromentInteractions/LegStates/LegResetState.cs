using UnityEngine;

using thisEState = LegStateMachine.ELegState; // shorthand

public class LegResetState : LegState
{
    //"Planted" state
    
    float _contactTime = 0;
    public LegResetState(LegContext lContext, thisEState estate) : base(lContext, estate)
    {

    }

    public override void EnterState()
    {
        _contactTime = 0;
    }
    public override void ExitState(){}
    public override void UpdateState()
    {
        _contactTime += Time.deltaTime;
    }
    public override thisEState GetNextState()
    {
        float resetTime = Co.ResetDur * Mathf.Pow(Co.ResetDurMod, Velocity.magnitude); // t / (1+k)^x
        if (_contactTime >= resetTime && Co.IsMoving())
        {
            Debug.Log("reset -> search");
            return thisEState.Search;
        }
        return StateKey;
    }
}