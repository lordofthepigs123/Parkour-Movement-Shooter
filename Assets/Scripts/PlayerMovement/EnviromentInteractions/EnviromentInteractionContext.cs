using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class EnviromentInteractionContext
{
    public enum EBodySide
    {
        LEFT,
        RIGHT
    }

    [Header("Specialized state machine control")]
    private Dictionary<EBodySide, TwoBoneIKConstraint> _legIkConstraint = new Dictionary<EBodySide, TwoBoneIKConstraint>();
    private Dictionary<EBodySide, TwoBoneIKConstraint> _armIkConstraint = new Dictionary<EBodySide, TwoBoneIKConstraint>();
    private Dictionary<EBodySide, Transform> _legTransform = new Dictionary<EBodySide, Transform>();    
    private Dictionary<EBodySide, Transform> _legTargetTransform = new Dictionary<EBodySide, Transform>();
    private Dictionary<EBodySide, Transform> _armTargetTransform = new Dictionary<EBodySide, Transform>();
    private MultiPositionConstraint _hipsConstraint;
    private EnviromentInteractionStateMachine _eism;
    private Transform _fwdAirSearchReference;
    private Transform _bwdAirSearchReference;
    private Rigidbody _rb;
    private Collider _rootCollider;
    private Transform _rootTransform;
    private MainRagdollHandeler _mr;
    private AnimationCurve _strideBACCurve;
    private AnimationCurve _strideFWDCurve;
    private AnimationCurve _strideVelToDisCurve;
    private AnimationCurve _footLiftCurve;
    private float _maxVelocityMod;
    private float _strideDisFallVel;
    private float _maxStepDownDis;
    private float _placeOffsetDis;
    private float _resetDur;
    private float _resetDurMod;
    private float _ikEnterDur;
    private float _ikExitDur;
    private float _minCompleteRatio;
    private LayerMask _groundLayer;
    private AnimationCurve _strideCurve;
    private AnimationCurve _footRotCurve;
    private AnimationCurve _strideHeightCurve;
    private float _minCenterDisplacement;
    private float _maxReverseSpeed;
    private float _speedLimiterThreshold;
    private float _stepDirThresholdBuf;
    private float _smoothNormalMult;
    private AnimationCurve _hipDisToHeight;
    private float _hipBounceSmooth;
    private float _hipPosSmooth;
    private float _hipLerpSmooth;
    private float _strechGive;
    private float _backRunDivisor;
    private float _maxAngleChange;
    private float _breakMult;
    private float _rayNormalFac;
    private float _slopeLowerMax;
    private float _footLength;
    private FootDetectManager _fd;
    private AnimationCurve _airPosLerpCurve;
    private AnimationCurve _airDisLegExtendCurve;
    private float _timeCap;
    private float _airRotLerpMult;
    private float _airHipMaxRepelDis;
    private float _airHipRepelMult;
    private float _airTipMaxRepelDis;
    private float _airTipRepelMult;
    private float _posLerpSpeedMod;
    private float _airFootAngleMult;
    private float _maxHipFlexVert;
    private float _maxHipFlexOff;
    private float _hipFlexMod;

    private RangedHandler _rh;
    private MeleeHandler _mh;

    //constructor
    public EnviromentInteractionContext(EnviromentInteractionStateMachine eism, MultiPositionConstraint hipsConstraint, TwoBoneIKConstraint leftLegIkConstraint, TwoBoneIKConstraint rightLegIkConstraint, TwoBoneIKConstraint leftArmIkConstraint, TwoBoneIKConstraint rightArmIkConstraint, Transform fwdAirSearchReference, Transform bwdAirSearchReference, Rigidbody rb,
    Collider rootCollider, Transform rootTransform, MainRagdollHandeler mr, LayerMask groundLayer, AnimationCurve strideBACCurve, AnimationCurve strideFWDCurve,
    AnimationCurve strideVelToDisCurve, AnimationCurve footLiftCurve, float maxVelocityMod, float strideDisFallVel, float maxStepDownDis, float placeOffsetDis,
    float resetDur, float resetDurMod, float ikEnterDur, float ikExitDur, float minCompleteRatio, AnimationCurve strideCurve, 
    AnimationCurve footRotCurve, AnimationCurve strideHeightCurve, float minCenterDisplacement, float maxReverseSpeed, float speedLimiterThreshold, float stepDirThresholdBuf,
    float smoothNormalMult, AnimationCurve hipDisToHeight, float hipBounceSmooth, float hipPosSmooth, float hipLerpSmooth, float strechGive, float backRunDivisor, float maxAngleChange, float breakMult, 
    float rayNormalFac, float slopeLowerMax, float footLength, FootDetectManager fd, AnimationCurve airPosLerpCurve, AnimationCurve airDisLegExtendCurve, float timeCap, float airRotLerpMult, float airHipMaxRepelDis, 
    float airHipRepelMult, float airTipMaxRepelDis, float airTipRepelMult, float posLerpSpeedMod, float airFootAngleMult, float maxHipFlexVert, float maxHipFlexOff, float hipFlexMod, RangedHandler rh, MeleeHandler mh)
    {
        _eism = eism;
        _rb = rb;
        _rootCollider = rootCollider;
        _rootTransform = rootTransform;
        _mr = mr;
        _groundLayer = groundLayer;
        _strideBACCurve = strideBACCurve;
        _strideFWDCurve = strideFWDCurve;
        _strideVelToDisCurve = strideVelToDisCurve;
        _footLiftCurve = footLiftCurve;
        _maxVelocityMod = maxVelocityMod;
        _strideDisFallVel = 1 + 2 / strideDisFallVel;
        _maxStepDownDis = maxStepDownDis;
        _placeOffsetDis = placeOffsetDis;
        _resetDur = resetDur;
        _resetDurMod = resetDurMod;
        _ikEnterDur = ikEnterDur;
        _ikExitDur = ikExitDur;
        _minCompleteRatio = minCompleteRatio;
        _strideCurve = strideCurve;
        _footRotCurve = footRotCurve;
        _strideHeightCurve = strideHeightCurve;
        _minCenterDisplacement = minCenterDisplacement;
        _maxReverseSpeed = maxReverseSpeed;
        _speedLimiterThreshold = speedLimiterThreshold;
        _stepDirThresholdBuf = stepDirThresholdBuf;
        _smoothNormalMult = smoothNormalMult;
        _hipDisToHeight = hipDisToHeight;
        _hipBounceSmooth = hipBounceSmooth;
        _hipPosSmooth = hipPosSmooth;
        _hipLerpSmooth = hipLerpSmooth;
        _strechGive = strechGive;
        _backRunDivisor = backRunDivisor;
        _maxAngleChange = maxAngleChange;
        _breakMult = breakMult;
        _rayNormalFac = rayNormalFac;
        _slopeLowerMax = slopeLowerMax;
        _footLength = footLength;
        _fd = fd;
        _airPosLerpCurve = airPosLerpCurve;
        _airDisLegExtendCurve = airDisLegExtendCurve;
        _timeCap = timeCap;
        _airRotLerpMult = airRotLerpMult;
        _airHipMaxRepelDis = airHipMaxRepelDis;
        _airHipRepelMult = airHipRepelMult;
        _airTipMaxRepelDis = airTipMaxRepelDis;
        _airTipRepelMult = airTipRepelMult;
        _posLerpSpeedMod = posLerpSpeedMod;
        _airFootAngleMult = airFootAngleMult;
        _maxHipFlexVert = maxHipFlexVert;
        _maxHipFlexOff = maxHipFlexOff;
        _hipFlexMod = hipFlexMod;

        _rh = rh;
        _mh = mh;

        //hips
        _hipsConstraint = hipsConstraint;
        OriginalHipPos = _hipsConstraint.data.sourceObjects[0].transform.localPosition;
        OriginalHipRot = _hipsConstraint.data.constrainedObject.localRotation;

        //legs
        _legIkConstraint.Add(EBodySide.LEFT, leftLegIkConstraint);
        _legIkConstraint.Add(EBodySide.RIGHT, rightLegIkConstraint);
        
        _legTransform.Add(EBodySide.LEFT, leftLegIkConstraint.data.root.transform);
        _legTransform.Add(EBodySide.RIGHT, rightLegIkConstraint.data.root.transform);
        _legTargetTransform.Add(EBodySide.LEFT, leftLegIkConstraint.data.target.transform);
        _legTargetTransform.Add(EBodySide.RIGHT, rightLegIkConstraint.data.target.transform);

        OppositeInvalidState.Add(EBodySide.LEFT, false);
        OppositeInvalidState.Add(EBodySide.RIGHT, false);
        StepNormal.Add(EBodySide.LEFT, Vector3.zero);
        StepNormal.Add(EBodySide.RIGHT, Vector3.zero);
        StaticNormal.Add(EBodySide.LEFT, Vector3.zero);
        StaticNormal.Add(EBodySide.RIGHT, Vector3.zero);

        _fwdAirSearchReference = fwdAirSearchReference;
        _bwdAirSearchReference = bwdAirSearchReference;

        LocalDesiStore.Add(EBodySide.LEFT, Vector3.zero);
        LocalDesiStore.Add(EBodySide.RIGHT, Vector3.zero);
        
        //TrackPoint.Add(EBodySide.LEFT, () => Fd.Left_trackPoint);
        //TrackPoint.Add(EBodySide.RIGHT, () => Fd.Right_trackPoint);

        LastStepDir = EStepDir.FORWARD;//Default

        //arms
        _armIkConstraint.Add(EBodySide.LEFT, leftArmIkConstraint);
        _armIkConstraint.Add(EBodySide.RIGHT, rightArmIkConstraint);
        _armTargetTransform.Add(EBodySide.LEFT, leftArmIkConstraint.data.target.transform);
        _armTargetTransform.Add(EBodySide.RIGHT, rightArmIkConstraint.data.target.transform);
    }

    // Read - only Propertise
    public Dictionary<EBodySide, TwoBoneIKConstraint> LegIkConstraint => _legIkConstraint;
    public Dictionary<EBodySide, TwoBoneIKConstraint> ArmIkConstraint => _armIkConstraint;
    public Dictionary<EBodySide, Transform> LegTransform => _legTransform;//hip
    public Dictionary<EBodySide, Transform> LegTargetTransform => _legTargetTransform;
    public Dictionary<EBodySide, Transform> ArmTargetTransform => _armTargetTransform;
    //public Dictionary<EBodySide, Func<Vector3>> TrackPoint = new Dictionary<EBodySide, Func<Vector3>>();
    public MultiPositionConstraint HipsConstraint => _hipsConstraint;
    public EnviromentInteractionStateMachine Eism => _eism;
    public Transform FwdAirSearchReference => _fwdAirSearchReference;
    public Transform BwdAirSearchReference => _bwdAirSearchReference;
    public Rigidbody Rb => _rb;
    public Collider RootCollider => _rootCollider;
    public Transform RootTransform => _rootTransform;
    public MainRagdollHandeler Mr => _mr;
    public PlayerColliderManager Cm => _mr.cm;
    public PlayerStateMachine Sm => _mr.sm;
    public LayerMask GroundLayer => _groundLayer;
    public AnimationCurve StrideBACCurve => _strideBACCurve;
    public AnimationCurve StrideFWDCurve => _strideFWDCurve;
    public AnimationCurve StrideVelToDisCurve => _strideVelToDisCurve;
    public AnimationCurve FootLiftCurve => _footLiftCurve;
    public float MaxVelocityMod => _maxVelocityMod;
    public float StrideDisFallVel => _strideDisFallVel;
    public float MaxStepDownDis => _maxStepDownDis;
    public float PlaceOffsetDis => _placeOffsetDis;
    public float ResetDur => _resetDur;
    public float ResetDurMod => _resetDurMod;
    public float IkEnterDur => _ikEnterDur;
    public float IkExitDur => _ikExitDur;
    public float MinCompleteRatio => _minCompleteRatio; // displacement before foot locks to postion and exits step
    public AnimationCurve StrideCurve => _strideCurve;
    public AnimationCurve FootRotCurve => _footRotCurve;
    public AnimationCurve StrideHeightCurve => _strideHeightCurve;
    public float MinCenterDisplacement => _minCenterDisplacement; // displacement before start move - should be Greater than MinActivePointDistance
    public float MaxReverseSpeed => _maxReverseSpeed;
    public float SpeedLimiterThreshold => _speedLimiterThreshold;
    public float StepDirThresholdBuf => _stepDirThresholdBuf;
    public float SmoothNormalMult => _smoothNormalMult;
    public AnimationCurve HipDisToHeight => _hipDisToHeight;
    public float HipBounceSmooth => _hipBounceSmooth;
    public float HipPosSmooth => _hipPosSmooth;
    public float HipLerpSmooth => _hipLerpSmooth;
    public float StrechGive => _strechGive;
    public float BackRunDivisor => _backRunDivisor;
    public float MaxAngleChange => _maxAngleChange;
    public float BreakMult => _breakMult;
    public float RayNormalFac => _rayNormalFac;
    public float SlopeLowerMax => _slopeLowerMax;
    public float FootLength => _footLength;
    public FootDetectManager Fd => _fd;
    public Vector3 FootDetectorTrans => Fd.Detector.position;
    public Vector3 TrackPoint => Fd.TrackPoint;
    public float TrackDot => Fd.TrackDot;
    public bool Tracking => Fd.Tracking;
    public AnimationCurve AirPosLerpCurve => _airPosLerpCurve;
    public AnimationCurve AirDisLegExtendCurve => _airDisLegExtendCurve;
    public float TimeCap => _timeCap;
    public float AirRotLerpMult => _airRotLerpMult;
    public float AirHipMaxRepelDis => _airHipMaxRepelDis;
    public float AirHipRepelMult => _airHipRepelMult;
    public float AirTipMaxRepelDis => _airTipMaxRepelDis;
    public float AirTipRepelMult => _airTipRepelMult;
    public float PosLerpSpeedMod => _posLerpSpeedMod;
    public float AirFootAngleMult => _airFootAngleMult;
    public float MaxHipFlexVert => _maxHipFlexVert;
    public float MaxHipFlexOff => _maxHipFlexOff;
    public float HipFlexMod => _hipFlexMod;
    public RangedHandler Rh => _rh;
    public MeleeHandler Mh => _mh; 

    //Set-able variables
    public Dictionary<EBodySide, bool> OppositeInvalidState = new Dictionary<EBodySide, bool>();
    public Dictionary<EBodySide, Vector3> StepNormal = new Dictionary<EBodySide, Vector3>(); // active normal of predicted step pos
    public Dictionary<EBodySide, Vector3> StaticNormal = new Dictionary<EBodySide, Vector3>(); // non zero when foot is locked on ground
    public Dictionary<EBodySide, Vector3> LocalDesiStore = new Dictionary<EBodySide, Vector3>();
    public enum EStepDir {FORWARD, BACKWARD}
    public Vector3 InstantPlayerNormal {get; private set;}
    public Vector3 SmoothPlayerNormal {get; private set;}
    public Vector3 OriginalHipPos {get; private set;}
    public Quaternion OriginalHipRot {get; private set;}
    public float LegLength;
    public float SpeedStrideRatio {get; private set;}
    public float FrontalStride {get; private set;}
    public float BackStride {get; private set;}
    public float ToMoveDisStride {get; private set;}
    public float FootLiftMult {get; private set;}
    public Vector3 LastRootDir {get; private set;}
    public EStepDir LastStepDir;
    public EBodySide LastStepSide;
    public Vector3 HipPos;
    public Vector3 HipNormal;
    public float LastDif;
    public Vector3 LerpOffset;
    public Vector3 AccelOffset;
    public Vector3 BounceOffset;
    public Vector3 LockedHipPosition; //target postion on current frame
    public Quaternion LockedHipRotation;

    public void GetPlayerNormal()
    {
        Vector3 normal1 = StaticNormal[EBodySide.LEFT];
        Vector3 normal2 = StaticNormal[EBodySide.RIGHT];
        Vector3 sum = Vector3.zero;
        if (normal1.magnitude == 0)
            normal1 = StepNormal[EBodySide.LEFT];
        if (normal2.magnitude == 0)
            normal2 = StepNormal[EBodySide.RIGHT];
        //average both foot normals
        sum += (normal1 + normal2).normalized;
        InstantPlayerNormal = sum; // ZERO is airborne

        //calc lerping
        if (InstantPlayerNormal.magnitude == 0)
            SmoothPlayerNormal = Vector3.zero;
        else
            SmoothPlayerNormal = Vector3.Lerp(SmoothPlayerNormal, InstantPlayerNormal, SmoothNormalMult * Time.deltaTime).normalized;
    }

    public void ResetPlayerNormal()
    {
        InstantPlayerNormal = Vector3.zero;
        SmoothPlayerNormal = Vector3.zero;
    }

    public Vector3 SFlatVelocity()
    {
        if (SmoothPlayerNormal.magnitude > 0)
            return Vector3.ProjectOnPlane(Rb.linearVelocity, SmoothPlayerNormal);
        return Rb.linearVelocity;
    }

    public void CalculateStride()
    {
        SpeedStrideRatio = Mathf.Clamp(SFlatVelocity().magnitude / MaxVelocityMod, 0, 1);
        //as speed increases overall stride length increase
        //but frontal(infront of player) stride decreases
        FrontalStride = StrideFWDCurve.Evaluate(SpeedStrideRatio);
        //and back(bahind player) stride increases
        BackStride = StrideBACCurve.Evaluate(SpeedStrideRatio);
        ToMoveDisStride = StrideVelToDisCurve.Evaluate(SpeedStrideRatio);
        FootLiftMult = FootLiftCurve.Evaluate(SpeedStrideRatio);
    }

    public void SaveLastRootDir()
    {
        LastRootDir = RootTransform.forward;
    }
}
