using UnityEngine;

using thisEState = EnviromentInteractionStateMachine.EEnviromentInteractionState; // shorthand

public class WalkState : EnviromentInteractionState
{
    public WalkState(EnviromentInteractionContext context, thisEState estate) : base(context, estate)
    {
        EnviromentInteractionContext Context = context;
    }

    public override void EnterState(){}
    public override void ExitState(){}
    public override void UpdateState()
    {
        
    }
    public override thisEState GetNextState()
    {
        return StateKey;
    }
}
