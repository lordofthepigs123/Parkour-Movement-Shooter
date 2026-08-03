using UnityEngine;
using UnityEngine.Animations.Rigging;

public class LegStateMachine : StateManager<LegStateMachine.ELegState>
{
    public void Init(EnviromentInteractionContext context, EnviromentInteractionContext.EBodySide side, EnviromentInteractionContext.EBodySide otherSide)
    {
        _context = context;
        _side = side;
        _otherSide = otherSide;

        LContext = new LegContext(this, _context, _side, _otherSide); 
        InitializeStates();
    }

    public enum ELegState
    {
        AirSearch,
        AirJump,
        AirAproach,
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
        States.Add(ELegState.AirJump, new LegAirJumpState(LContext, ELegState.AirJump));
        States.Add(ELegState.AirAproach, new LegAirAproachState(LContext, ELegState.AirAproach));
        States.Add(ELegState.Search, new LegSearchState(LContext, ELegState.Search));
        States.Add(ELegState.Step, new LegStepState(LContext, ELegState.Step));
        States.Add(ELegState.BackStep, new LegBackStepState(LContext, ELegState.BackStep));

         // Set first state
        CurrentStateKey = ELegState.AirSearch;
        CurrentState = States[CurrentStateKey];
    }

/*
    private void setupCollider()
    {
        _triggerCollider.size = new Vector3(1,1,1);
        _triggerCollider.center = new Vector3(_rootCollider.center.x, _rootCollider.center.y + 1, _rootCollider.center.z + 1);
    }
*/


}
