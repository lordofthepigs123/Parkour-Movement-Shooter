using System.Collections.Generic;
using System;
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
    private float _speedLimiterThreshold;
    private float _stepDirThresholdBuf;
    private float _smoothNormalMult;
    private AnimationCurve _hipDisToHeight;
    private float _hipBounceSmooth;
    private float _strechGive;
    private float _backRunDivisor;
    private float _maxAngleChange;

    private RangedHandler _rh;
    private MeleeHandler _mh;

    //constructor
    public EnviromentInteractionContext(MultiPositionConstraint hipsConstraint, TwoBoneIKConstraint leftLegIkConstraint, TwoBoneIKConstraint rightLegIkConstraint, TwoBoneIKConstraint leftArmIkConstraint, TwoBoneIKConstraint rightArmIkConstraint, Rigidbody rb,
    Collider rootCollider, Transform rootTransform, MainRagdollHandeler mr, LayerMask groundLayer, AnimationCurve strideBACCurve, AnimationCurve strideFWDCurve,
    AnimationCurve strideVelToDisCurve, AnimationCurve footLiftCurve, float maxVelocityMod, float strideDisFallVel, float maxStepDownDis, float placeOffsetDis,
    float resetDur, float resetDurMod, float ikEnterDur, float ikExitDur, float minCompleteRatio, AnimationCurve strideCurve, 
    AnimationCurve footRotCurve, AnimationCurve strideHeightCurve, float minCenterDisplacement, float speedLimiterThreshold, float stepDirThresholdBuf,
    float smoothNormalMult, AnimationCurve hipDisToHeight, float hipBounceSmooth, float strechGive, float backRunDivisor, float maxAngleChange, RangedHandler rh, MeleeHandler mh)
    {
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
        _speedLimiterThreshold = speedLimiterThreshold;
        _stepDirThresholdBuf = stepDirThresholdBuf;
        _smoothNormalMult = smoothNormalMult;
        _hipDisToHeight = hipDisToHeight;
        _hipBounceSmooth = hipBounceSmooth;
        _strechGive = strechGive;
        _backRunDivisor = backRunDivisor;
        _maxAngleChange = maxAngleChange;

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
    public MultiPositionConstraint HipsConstraint => _hipsConstraint;
    public Rigidbody Rb => _rb;
    public Collider RootCollider => _rootCollider;
    public Transform RootTransform => _rootTransform;
    public MainRagdollHandeler Mr => _mr;
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
    public float SpeedLimiterThreshold => _speedLimiterThreshold;
    public float StepDirThresholdBuf => _stepDirThresholdBuf;
    public float SmoothNormalMult => _smoothNormalMult;
    public AnimationCurve HipDisToHeight => _hipDisToHeight;
    public float HipBounceSmooth => _hipBounceSmooth;
    public float StrechGive => _strechGive;
    public float BackRunDivisor => _backRunDivisor;
    public float MaxAngleChange => _maxAngleChange;
    public RangedHandler Rh => _rh;
    public MeleeHandler Mh => _mh; 

    //Set-able variables
    public Dictionary<EBodySide, bool> OppositeInvalidState = new Dictionary<EBodySide, bool>();
    public Dictionary<EBodySide, Vector3> StepNormal = new Dictionary<EBodySide, Vector3>(); // active normal of predicted step pos
    public Dictionary<EBodySide, Vector3> StaticNormal = new Dictionary<EBodySide, Vector3>(); // non zero when foot is locked on ground
    public enum EStepDir {FORWARD, BACKWARD}
    public Vector3 InstantPlayerNormal {get; private set;}
    public Vector3 SmoothPlayerNormal {get; private set;}
    public Vector3 OriginalHipPos {get; private set;}
    public Quaternion OriginalHipRot {get; private set;}
    public float FrontalStride {get; private set;}
    public float BackStride {get; private set;}
    public float ToMoveDisStride {get; private set;}
    public float FootLiftMult {get; private set;}
    public EStepDir LastStepDir;
    public EBodySide LastStepSide;
    public Vector3 HipPos;
    public Vector3 HipNormal;
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

    public Vector3 SFlatVelocity()
    {
        if (SmoothPlayerNormal.magnitude > 0)
            return Vector3.ProjectOnPlane(Rb.linearVelocity, SmoothPlayerNormal);
        return Rb.linearVelocity;
    }

    public void CalculateStride()
    {
        float tempRatio = Mathf.Clamp(SFlatVelocity().magnitude / MaxVelocityMod, 0, 1);
        //as speed increases overall stride length increase
        //but frontal(infront of player) stride decreases
        FrontalStride = StrideFWDCurve.Evaluate(tempRatio);
        //and back(bahind player) stride increases
        BackStride = StrideBACCurve.Evaluate(tempRatio);
        ToMoveDisStride = StrideVelToDisCurve.Evaluate(tempRatio);
        FootLiftMult = FootLiftCurve.Evaluate(tempRatio);
    }
}
