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
    private ArmStateMachine _leftHandMac;
    private ArmStateMachine _rightHandMac;
    private ArmContext _leftAContext;
    private ArmContext _rightAContext;

    [Header("State Machine control")]
    [SerializeField] private MultiPositionConstraint _hipsConstraint;
    [SerializeField] private TwoBoneIKConstraint _leftLegIkConstraint;
    [SerializeField] private TwoBoneIKConstraint _rightLegIkConstraint;
    [SerializeField] private TwoBoneIKConstraint _leftArmIkConstraint;
    [SerializeField] private TwoBoneIKConstraint _rightArmIkConstraint;
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private Transform _rootBodyTransform;
    [SerializeField] private CapsuleCollider _rootCollider;
    //[SerializeField] private BoxCollider _triggerCollider;
    
    [Header("Striding control")]
    [SerializeField] private MainRagdollHandeler _mr;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _maxVelocityMod;
    [SerializeField] private AnimationCurve _strideBACCurve;
    [SerializeField] private AnimationCurve _strideFWDCurve;
    [SerializeField] private AnimationCurve _strideVelToDisCurve;
    [SerializeField] private AnimationCurve _footLiftCurve;
    [SerializeField] private float _strideDisFallVel;
    [SerializeField] private float _maxStepDownDis;
    [SerializeField] private float _placeOffsetDis;
    [SerializeField] private float _resetDur;
    [SerializeField][Range(1,2)] private float _resetDurMod; 
    [SerializeField] private float _ikEnterDur;
    [SerializeField] private float _ikExitDur;
    [SerializeField] [Range(0,1)] private float _minCompleteRatio;
    [SerializeField] private AnimationCurve _strideCurve;
    [SerializeField] private AnimationCurve _footRotCurve;
    [SerializeField] private AnimationCurve _strideHeightCurve;
    [SerializeField] private float _minCenterDisplacement;
    [SerializeField] private float _speedLimiterThreshold;
    [SerializeField] private float _stepDirThresholdBuf;
    [SerializeField] private float _smoothNormalMult;
    [SerializeField] private AnimationCurve _hipDisToHeight;
    [SerializeField] private float _hipBounceSmooth;
    [SerializeField] private float _strechGive;
    [SerializeField] private float _backRunDivisor;
    [SerializeField] private float _maxAngleChange;
    [Header("Arm control")]
    [SerializeField] private RangedHandler _rh;
    [SerializeField] private MeleeHandler _mh;

    private void Start()
    {
        _context = new EnviromentInteractionContext(_hipsConstraint, _leftLegIkConstraint, _rightLegIkConstraint, _leftArmIkConstraint, _rightArmIkConstraint, _rb, _rootCollider, _rootBodyTransform, _mr, _groundLayer,
        _strideBACCurve, _strideFWDCurve, _strideVelToDisCurve, _footLiftCurve, _maxVelocityMod, _strideDisFallVel,
        _maxStepDownDis, _placeOffsetDis, _resetDur, _resetDurMod, _ikEnterDur, _ikExitDur, _minCompleteRatio, _strideCurve,
        _footRotCurve, _strideHeightCurve, _minCenterDisplacement, _speedLimiterThreshold, _stepDirThresholdBuf, _smoothNormalMult, _hipDisToHeight, _hipBounceSmooth,
        _strechGive, _backRunDivisor, _maxAngleChange, _rh, _mh);
        
        //create new leg state machines and reference their context info
        _leftFootMac = gameObject.AddComponent<LegStateMachine>();
        _leftFootMac.Init(_context, EnviromentInteractionContext.EBodySide.LEFT, EnviromentInteractionContext.EBodySide.RIGHT);
        _rightFootMac = gameObject.AddComponent<LegStateMachine>();
        _rightFootMac.Init(_context, EnviromentInteractionContext.EBodySide.RIGHT, EnviromentInteractionContext.EBodySide.LEFT);
        _leftLContext = _leftFootMac.LContext;
        _rightLContext = _rightFootMac.LContext;

        //create new arm state machines and reference their context info
        _leftHandMac = gameObject.AddComponent<ArmStateMachine>();
        _leftHandMac.Init(_context, EnviromentInteractionContext.EBodySide.LEFT, EnviromentInteractionContext.EBodySide.RIGHT);
        _rightHandMac = gameObject.AddComponent<ArmStateMachine>();
        _rightHandMac.Init(_context, EnviromentInteractionContext.EBodySide.RIGHT, EnviromentInteractionContext.EBodySide.LEFT);
        _leftAContext = _leftHandMac.AContext;
        _rightAContext = _rightHandMac.AContext;

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
