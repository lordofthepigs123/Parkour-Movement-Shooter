using UnityEngine;

public class ArmStateMachine : StateManager<ArmStateMachine.EArmState>
{
    public void Init(EnviromentInteractionContext context, EnviromentInteractionContext.EBodySide side, EnviromentInteractionContext.EBodySide otherSide)
    {
        _context = context;
        _side = side;
        _otherSide = otherSide;

        AContext = new ArmContext(this, _context, _side, _otherSide); 
        InitializeStates();
    }

    public enum EArmState
    {
        Tracking,
        Free
    }

    private EnviromentInteractionContext _context;
    private EnviromentInteractionContext.EBodySide _side;
    private EnviromentInteractionContext.EBodySide _otherSide;
    public ArmContext AContext;

    private void InitializeStates() // Add States to inherited StateManager dictionary and set initial state
    {
        States.Add(EArmState.Tracking, new ArmTrackingState(AContext, EArmState.Tracking));
        States.Add(EArmState.Free, new ArmFreeState(AContext, EArmState.Free));

        // Set first state
        CurrentStateKey = EArmState.Tracking;
        CurrentState = States[CurrentStateKey];
    }
}
