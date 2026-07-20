using UnityEngine;
using DG.Tweening;

using Side = EnviromentInteractionContext.EBodySide; //shorthand

public abstract class EnviromentInteractionState : BaseState<EnviromentInteractionStateMachine.EEnviromentInteractionState>
{
    protected EnviromentInteractionContext Context;

    public EnviromentInteractionState(EnviromentInteractionContext context, EnviromentInteractionStateMachine.EEnviromentInteractionState stateKey) : base(stateKey)
    {
        Context = context;
    }

    //inheritable methods for affecting target IK
    protected void StartIkTargetPositionTracking()
    {
        DOTween.To(() => Context.LegIkConstraint[Side.LEFT].weight, x => Context.LegIkConstraint[Side.LEFT].weight = x, 1, Context.IkEnterDur);
        DOTween.To(() => Context.LegIkConstraint[Side.RIGHT].weight, x => Context.LegIkConstraint[Side.RIGHT].weight = x, 1, Context.IkEnterDur);
    }
    protected void UpdateIkTargetPosition()
    {
        
    }
    protected void ResetIkTargetPositionTracking()
    {
        DOTween.To(() => Context.LegIkConstraint[Side.LEFT].weight, x => Context.LegIkConstraint[Side.LEFT].weight = x, 0, Context.IkExitDur);
        DOTween.To(() => Context.LegIkConstraint[Side.RIGHT].weight, x => Context.LegIkConstraint[Side.RIGHT].weight = x, 0, Context.IkExitDur);
    }

    protected void SetHipTarget(Vector3 localPosition, Vector3 normal)
    {
        Context.LockedHipPosition = localPosition + Context.OriginalHipPos;
        Context.LockedHipRotation = Quaternion.FromToRotation(Vector3.up, normal) * Context.OriginalHipRot;
    }

    protected void HoldHipTarget()
    {
        Transform targetTrans = Context.HipsConstraint.data.sourceObjects[0].transform;
        targetTrans.localPosition = Context.LockedHipPosition;
        targetTrans.localRotation = Context.LockedHipRotation;
    }
}
