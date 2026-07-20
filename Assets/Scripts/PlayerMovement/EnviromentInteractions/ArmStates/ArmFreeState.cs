using System;
using UnityEngine;
using thisEState = ArmStateMachine.EArmState; // shorthand

public class ArmFreeState : ArmState
{
    public ArmFreeState(ArmContext aContext, thisEState estate) : base(aContext, estate)
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
        
    }
    public override thisEState GetNextState()
    {
        return StateKey;
    }
}
