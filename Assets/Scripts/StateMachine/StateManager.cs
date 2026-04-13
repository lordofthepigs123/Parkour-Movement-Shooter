using System.Collections.Generic;
using System;
using UnityEngine;

public abstract class StateManager<EState> : MonoBehaviour where EState : Enum
{
    //Contains collection of all states & associated key to reference and set CurrentState
    protected Dictionary<EState, BaseState<EState>> States = new Dictionary<EState, BaseState<EState>>();

    protected BaseState<EState> CurrentState;
    protected bool IsTransitioningState = true;

    void Start()
    {
        CurrentState.EnterState();
    }

    void Update()
    {
        EState nextStateKey = CurrentState.GetNextState();

        if (CurrentState.Equals(CurrentState.StateKey) && !IsTransitioningState) // Run continous update of current state or transition to new
        {
            CurrentState.UpdateState();
        }
        else
        {
            TransitionToState(nextStateKey);
        }
    }

    public void TransitionToState(EState stateKey)
    {
        IsTransitioningState = true;
        CurrentState.ExitState(); // run state exit
        CurrentState = States[stateKey];
        CurrentState.EnterState(); // run state enter
        IsTransitioningState = false;
    }

    void OnTriggerEnter(Collider other)
    {
        CurrentState.OnTriggerEnter(other);
    }

    void OnTriggerStay(Collider other)
    {
        CurrentState.OnTriggerStay(other);
    }

    void OnTriggerExit(Collider other)
    {
        CurrentState.OnTriggerExit(other);
    }
}
