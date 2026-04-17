using UnityEngine;

using thisEState = EnviromentInteractionStateMachine.EEnviromentInteractionState; // shorthand

public class LegSearchState : EnviromentInteractionState
{
    public LegSearchState(EnviromentInteractionContext context, thisEState estate) : base(context, estate)
    {
        EnviromentInteractionContext Context = context;
    }

    public override void EnterState(){}
    public override void ExitState(){}
    public override void UpdateState(){}
    public override thisEState GetNextState()
    {
        return StateKey;
    }
    public override void OnTriggerEnter(Collider other)
    {
        StartIkTargetPositionTracking(other);
    }
    public override void OnTriggerStay(Collider other)
    {
        UpdateIkTargetPosition(other);
    }
    public override void OnTriggerExit(Collider other)
    {
        ResetIkTargetPositionTracking(other);
    }
} 