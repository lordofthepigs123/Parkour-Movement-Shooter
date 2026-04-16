using UnityEngine;
using UnityEngine.Animations.Rigging;

public class LegStateMachine : StateManager<LegStateMachine.ELegState>
{
    public LegStateMachine(EnviromentInteractionContext context, EnviromentInteractionContext.EBodySide side)
    {
        _context = context;
        _side = side;
    }

    public enum ELegState
    {
        Search,
        Step,
        Reset
    }

    private EnviromentInteractionContext _context;
    private EnviromentInteractionContext.EBodySide _side;
    public LegContext LContext;

    [Header("State Machine control")]
    [SerializeField] private TwoBoneIKConstraint _leftIkConstraint;
    [SerializeField] private TwoBoneIKConstraint _rightIkConstraint;
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private CapsuleCollider _rootCollider;
    [SerializeField] private BoxCollider _triggerCollider;
    
    [Header("Physics Simulation Tweaks")]

    [SerializeField] private MainRagdollHandeler _mr;
    [SerializeField] private float _maxStrideDisBAC;
    [SerializeField] private float _minStrideDisBAC;
    [SerializeField] private float _maxStrideDisFWD;
    [SerializeField] private float _minStrideDisFWD;
    [SerializeField] private float _strideDisFallVel;
    [SerializeField] private float _strideDisFalloff;
    

    private void Awake()
    {
        LContext = new LegContext(_context, _side);
        InitializeStates();
    }

    private void InitializeStates() // Add States to inherited StateManager dictionary and set initial state
    {
        //States.Add(EEnviromentInteractionState.Walk, new WalkState(_context, EEnviromentInteractionState.Search)); # 
        //States.Add(EEnviromentInteractionState.Step, new WalkStepState(_context, EEnviromentInteractionState.Step));
        //States.Add(EEnviromentInteractionState.Reset, new WalkResetState(_context, EEnviromentInteractionState.Reset));

        CurrentState = States[ELegState.Reset]; // Set first state
    }

/*
    private void setupCollider()
    {
        _triggerCollider.size = new Vector3(1,1,1);
        _triggerCollider.center = new Vector3(_rootCollider.center.x, _rootCollider.center.y + 1, _rootCollider.center.z + 1);
    }
*/


}
