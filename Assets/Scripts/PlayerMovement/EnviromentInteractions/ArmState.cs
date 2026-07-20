using UnityEngine;
using UnityEngine.Animations.Rigging;

public abstract class ArmState : BaseState<ArmStateMachine.EArmState>
{
    //The Side variable of class instance is set as = LContext.Side
    protected ArmContext AContext;
    protected EnviromentInteractionContext Co;//shorthand
    protected TwoBoneIKConstraint Constraint;
    protected Vector3 CurrentNormal;


    public ArmState(ArmContext aContext, ArmStateMachine.EArmState stateKey) : base(stateKey)
    {
        AContext = aContext;
        //setup
        Co = AContext.Context;//shorthand
        Constraint = AContext.ThisIkConstraint;
    }


    protected void ResetIkTargetPositionTracking()
    {
        Co.StepNormal[AContext.Side] = Vector3.zero;
        AContext.LockedPosition = Vector3.zero;
        AContext.LockedRotation = AContext.OriginalHandRot;
    }


    protected void SetIkTarget(Vector3 position, Quaternion rotation)
    {
        AContext.LockedPosition = position;
        AContext.LockedRotation = rotation;
    }

    protected void HoldIkTarget()
    {
        AContext.ThisTargetTransform.position = AContext.LockedPosition;
        AContext.ThisTargetTransform.rotation = AContext.LockedRotation * AContext.OriginalHandRot;
    }

}

