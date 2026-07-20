using UnityEngine;
using UnityEngine.Animations.Rigging;

public class LegStateMachine : StateManager<LegStateMachine.ELegState>
{
    public void Init(EnviromentInteractionContext context, EnviromentInteractionContext.EBodySide side, EnviromentInteractionContext.EBodySide otherSide)
    {
        _context = context;
        _side = side;
        _otherSide = otherSide;

        LContext = new LegContext(_context, _side, _otherSide); 
        InitializeStates();
    }

    public enum ELegState
    {
        AirSearch,
        Search,
        Step,
        BackStep
    }

    private EnviromentInteractionContext _context;
    private EnviromentInteractionContext.EBodySide _side;
    private EnviromentInteractionContext.EBodySide _otherSide;
    public LegContext LContext;

    private void InitializeStates() // Add States to inherited StateManager dictionary and set initial state
    {
        States.Add(ELegState.AirSearch, new LegAirSearchState(LContext, ELegState.AirSearch));
        States.Add(ELegState.Search, new LegSearchState(LContext, ELegState.Search));
        States.Add(ELegState.Step, new LegStepState(LContext, ELegState.Step));
        States.Add(ELegState.BackStep, new LegBackStepState(LContext, ELegState.BackStep));

        CurrentState = States[ELegState.AirSearch]; // Set first state
    }

/*
    private void setupCollider()
    {
        _triggerCollider.size = new Vector3(1,1,1);
        _triggerCollider.center = new Vector3(_rootCollider.center.x, _rootCollider.center.y + 1, _rootCollider.center.z + 1);
    }
*/


}
