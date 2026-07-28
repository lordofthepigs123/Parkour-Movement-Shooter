using System;
using UnityEngine;
using thisEState = ArmStateMachine.EArmState; // shorthand

public class ArmTrackingState : ArmState
{
    public ArmTrackingState(ArmContext aContext, thisEState estate) : base(aContext, estate)
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
        if (AContext.Side == EnviromentInteractionContext.EBodySide.LEFT)
        {
            AContext.LockedPosition = Co.Mh.Melee.position;
            AContext.LockedRotation = Co.Mh.Melee.rotation;
        }
        else
        {
            AContext.LockedPosition = Co.Rh.Ranged.position;
            AContext.LockedRotation = Co.Rh.Ranged.rotation;
        }
        SetIkTarget(AContext.LockedPosition, AContext.LockedRotation);
        HoldIkTarget();
    }
    public override void LateUpdateState(){}
    public override thisEState GetNextState()
    {
        return StateKey;
    }
}
