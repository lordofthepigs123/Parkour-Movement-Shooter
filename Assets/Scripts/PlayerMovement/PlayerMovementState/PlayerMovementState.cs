using UnityEngine;

public abstract class PlayerMovementState : BaseState<PlayerStateMachine.EMovementState>
{
    //The Side variable of class instance is set as = LContext.Side
    [SerializeField] protected bool Freeze;
    [SerializeField] protected float MoveSpeed;
    [SerializeField] protected bool FwdLocked;
    protected Rigidbody Rb;
    protected PlayerMovement Pm;
    protected PlayerCam Pc;

    public PlayerMovementState(Rigidbody rb, PlayerMovement pm, PlayerCam pc ,PlayerStateMachine.EMovementState stateKey) : base(stateKey)
    {
        Rb = rb;
        Pm = pm;
        Pc = pc;
    }

    public override void EnterState()
    {
        if (Freeze)
            Rb.linearVelocity = Vector3.zero;
        Pm.moveSpeed = MoveSpeed;
        Pc.fwdLocked = FwdLocked;
    }

}
