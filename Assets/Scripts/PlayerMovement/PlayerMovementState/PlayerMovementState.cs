using UnityEngine;

public abstract class PlayerMovementState : BaseState<PlayerStateMachine.EMovementState>
{
    //The Side variable of class instance is set as = LContext.Side
    [SerializeField] protected bool Freeze;
    [SerializeField] protected float MoveSpeed;
    [SerializeField] protected bool FwdLocked;
    protected Rigidbody Rb;
    protected PlayerCam Pc;
    protected MovementContext Context;

    public PlayerMovementState(MovementContext _context ,Rigidbody rb, PlayerCam pc ,PlayerStateMachine.EMovementState stateKey) : base(stateKey)
    {
        Context = _context;
        Rb = rb;
        Pc = pc;
    }

    public override void EnterState()
    {
        if (Freeze)
            Rb.linearVelocity = Vector3.zero;
        Context.moveSpeed = MoveSpeed;
        Pc.fwdLocked = FwdLocked;
    }

}
