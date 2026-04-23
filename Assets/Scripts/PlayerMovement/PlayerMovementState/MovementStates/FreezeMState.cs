using UnityEngine;
using thisEState = PlayerStateMachine.EMovementState; // shorthand
public class FreezeMState : PlayerMovementState
{
    public FreezeMState(Rigidbody rb, PlayerMovement pm, PlayerCam pc, thisEState estate) : base(rb, pm, pc, estate)
    {

    }

    public override void ExitState(){}
    public override void UpdateState(){}
    public override thisEState GetNextState()
    {
        return StateKey;
    }
} 