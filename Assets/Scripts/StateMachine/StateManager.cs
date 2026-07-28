using System.Collections.Generic;
using System;
using UnityEngine;

public abstract class StateManager<EState> : MonoBehaviour where EState : Enum
{
    //Contains collection of all states & associated key to reference and set CurrentState
    protected Dictionary<EState, BaseState<EState>> States = new Dictionary<EState, BaseState<EState>>();
    protected BaseState<EState> CurrentState;
    protected bool IsTransitioningState = false;
    public EState CurrentStateKey {get; protected set;}
    protected EState nextStateKey;

    private void Start()
    {
        CurrentState.EnterState();
    }

    private void Update()
    {
        nextStateKey = CurrentState.GetNextState();

        if (nextStateKey.Equals(CurrentState.StateKey) && !IsTransitioningState) // Run continous update of current state or transition to new
        {
            CurrentState.UpdateState();
        }
        else
        {
            TransitionToState(nextStateKey);
        }
    }

    private void LateUpdate()
    {
        if (CurrentState != null && nextStateKey.Equals(CurrentState.StateKey) && !IsTransitioningState)
        {
            CurrentState.LateUpdateState();
        }
    }

    public void TransitionToState(EState stateKey)
    {
        IsTransitioningState = true;
        CurrentState.ExitState(); // run state exit
        CurrentStateKey = stateKey;
        CurrentState = States[stateKey];
        CurrentState.EnterState(); // run state enter
        IsTransitioningState = false;
    }
}
