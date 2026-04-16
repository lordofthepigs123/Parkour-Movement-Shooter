using UnityEngine;

public abstract class EnviromentInteractionState : BaseState<EnviromentInteractionStateMachine.EEnviromentInteractionState>
{
    protected EnviromentInteractionContext Context;

    public EnviromentInteractionState(EnviromentInteractionContext context, EnviromentInteractionStateMachine.EEnviromentInteractionState stateKey) : base(stateKey)
    {
        Context = context;
    }

    //inheritable methods for affecting target IK
    protected void StartIkTargetPositionTracking(Collider intersectingCollider)
    {

    }
    protected void UpdateIkTargetPosition(Collider intersectingCollider)
    {
        
    }
    protected void ResetIkTargetPositionTracking(Collider intersectingCollider)
    {
        
    }

}
