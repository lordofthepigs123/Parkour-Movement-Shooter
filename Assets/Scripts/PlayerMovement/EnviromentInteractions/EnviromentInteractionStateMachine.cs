using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class EnviromentInteractionStateMachine : StateManager<EnviromentInteractionStateMachine.EEnviromentInteractionState>
{
    public enum EEnviromentInteractionState
    {
        Walk,
        Air,
        Swing,
        Animation,
    }

    private EnviromentInteractionContext _context;
    private LegStateMachine _leftFootMac;
    private LegStateMachine _rightFootMac;
    private LegContext _leftLContext;
    private LegContext _rightLContext;

    [Header("State Machine control")]
    [SerializeField] private TwoBoneIKConstraint _leftIkConstraint;
    [SerializeField] private TwoBoneIKConstraint _rightIkConstraint;
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private CapsuleCollider _rootCollider;
    //[SerializeField] private BoxCollider _triggerCollider;
    
    [Header("Physics Simulation Tweaks")]

    [SerializeField] private MainRagdollHandeler _mr;
    [SerializeField] private float _maxStrideDisBAC;
    [SerializeField] private float _minStrideDisBAC;
    [SerializeField] private float _maxStrideDisFWD;
    [SerializeField] private float _minStrideDisFWD;
    [SerializeField] private float _strideDisFallVel;
    [SerializeField] private float _maxStepDownDis;
    [SerializeField] private float _placeOffsetDis;
    [SerializeField] private float _resetDur;
    [SerializeField][Range(1,2)] private float _resetDurMod; 
    [SerializeField] private float _ikEnterDur;
    [SerializeField] private float _ikExitDur;
    [SerializeField] private float _minActivePointDistance;
    [SerializeField] private LayerMask _wallLayer;

    private void Awake()
    {
        _context = new EnviromentInteractionContext(_leftIkConstraint, _rightIkConstraint, _rb, _rootCollider, transform.root, _mr,
         _maxStrideDisBAC, _minStrideDisBAC, _maxStrideDisFWD, _minStrideDisFWD, _strideDisFallVel,
          _maxStepDownDis, _placeOffsetDis, _resetDur, _resetDurMod, _ikEnterDur, _ikExitDur, _minActivePointDistance, _wallLayer);
        
        //create new leg state machines and reference their context info
        Debug.Log("begin");
        _leftFootMac = this.AddComponent<LegStateMachine>();
        _leftFootMac.Init(_context, EnviromentInteractionContext.EBodySide.LEFT, EnviromentInteractionContext.EBodySide.RIGHT);
        Debug.Log("check");
        _rightFootMac = this.AddComponent<LegStateMachine>();
        _rightFootMac.Init(_context, EnviromentInteractionContext.EBodySide.RIGHT, EnviromentInteractionContext.EBodySide.LEFT);
        _leftLContext = _leftFootMac.LContext;
        _rightLContext = _rightFootMac.LContext;

        InitializeStates();
    }

    private void InitializeStates() // Add States to inherited StateManager dictionary and set initial state
    {
        States.Add(EEnviromentInteractionState.Walk, new WalkState(_context, EEnviromentInteractionState.Walk));
        //States.Add(EEnviromentInteractionState.Step, new WalkStepState(_context, EEnviromentInteractionState.Step)); # 
        //States.Add(EEnviromentInteractionState.Reset, new WalkResetState(_context, EEnviromentInteractionState.Reset));

        CurrentState = States[EEnviromentInteractionState.Walk]; // Set first state
    }

/*
    private void setupCollider()
    {
        _triggerCollider.size = new Vector3(1,1,1);
        _triggerCollider.center = new Vector3(_rootCollider.center.x, _rootCollider.center.y + 1, _rootCollider.center.z + 1);
    }
*/


}
