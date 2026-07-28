using UnityEngine;
using thisEState = PlayerStateMachine.EMovementState; // shorthand
public class FreezeMState : PlayerMovementState
{
    public FreezeMState(MovementContext _context, Rigidbody rb, PlayerCam pc, thisEState estate) : base(_context ,rb, pc, estate)
    {

    }

    public override void ExitState(){}
    public override void UpdateState(){}
    public override void LateUpdateState(){}
    public override thisEState GetNextState()
    {
        return StateKey;
    }
} 