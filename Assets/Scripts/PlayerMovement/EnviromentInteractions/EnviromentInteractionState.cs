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
        DOTween.To(() => Context.IkConstraint[Side.LEFT].weight, x => Context.IkConstraint[Side.LEFT].weight = x, 1, Context.IkEnterDur);
        DOTween.To(() => Context.IkConstraint[Side.RIGHT].weight, x => Context.IkConstraint[Side.RIGHT].weight = x, 1, Context.IkEnterDur);
    }
    protected void UpdateIkTargetPosition()
    {
        
    }
    protected void ResetIkTargetPositionTracking()
    {
        DOTween.To(() => Context.IkConstraint[Side.LEFT].weight, x => Context.IkConstraint[Side.LEFT].weight = x, 0, Context.IkExitDur);
        DOTween.To(() => Context.IkConstraint[Side.RIGHT].weight, x => Context.IkConstraint[Side.RIGHT].weight = x, 0, Context.IkExitDur);
    }

}
