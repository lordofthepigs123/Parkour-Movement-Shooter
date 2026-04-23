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
    private Dictionary<EBodySide, TwoBoneIKConstraint> _ikConstraint = new Dictionary<EBodySide, TwoBoneIKConstraint>();
    private Dictionary<EBodySide, Transform> _legTransform = new Dictionary<EBodySide, Transform>();
    private Dictionary<EBodySide, Transform> _targetTransform = new Dictionary<EBodySide, Transform>();
    private Rigidbody _rb;
    private Collider _rootCollider;
    private Transform _rootTransform;
    private MainRagdollHandeler _mr;
    private float _difStrideDisBAC;
    private float _maxStrideDisBAC;
    private float _difStrideDisFWD;
    private float _minStrideDisFWD;
    private float _strideDisFallVel;
    private float _maxStepDownDis;
    private float _placeOffsetDis;
    private float _resetDur;
    private float _resetDurMod;
    private float _ikEnterDur;
    private float _ikExitDur;
    private float _minActivePointDistance;
    private LayerMask _wallLayer;

    //constructor
    public EnviromentInteractionContext(TwoBoneIKConstraint leftIkConstraint, TwoBoneIKConstraint rightIkConstraint, Rigidbody rb,
     Collider rootCollider, Transform rootTransform, MainRagdollHandeler mr, float maxStrideDisBAC, float minStrideDisBAC,
      float maxStrideDisFWD, float minStrideDisFWD, float strideDisFallVel, float maxStepDownDis, float placeOffsetDis
      , float resetDur, float resetDurMod, float ikEnterDur, float ikExitDur, float minActivePointDistance, LayerMask wallLayer)
    {
        _rb = rb;
        _rootCollider = rootCollider;
        _rootTransform = rootTransform;
        _mr = mr;
        _maxStrideDisBAC = maxStrideDisBAC;
        _difStrideDisBAC = maxStrideDisBAC - minStrideDisBAC;
        _difStrideDisFWD = maxStrideDisFWD - minStrideDisFWD;
        _minStrideDisFWD = minStrideDisFWD;
        _strideDisFallVel = 1 + 2 / strideDisFallVel;
        _maxStepDownDis = maxStepDownDis;
        _placeOffsetDis = placeOffsetDis;
        _resetDur = resetDur;
        _resetDurMod = resetDurMod;
        _ikEnterDur = ikEnterDur;
        _ikExitDur = ikExitDur;
        _minActivePointDistance = minActivePointDistance;
        _wallLayer = wallLayer;

        _ikConstraint.Add(EBodySide.LEFT, leftIkConstraint);
        _ikConstraint.Add(EBodySide.RIGHT, rightIkConstraint);
        
        _legTransform.Add(EBodySide.LEFT, leftIkConstraint.data.root.transform);
        _legTransform.Add(EBodySide.RIGHT, rightIkConstraint.data.root.transform);
        _targetTransform.Add(EBodySide.LEFT, leftIkConstraint.data.target.transform);
        _targetTransform.Add(EBodySide.RIGHT, rightIkConstraint.data.target.transform);

        OppositeInvalidState.Add(EBodySide.LEFT, false);
        OppositeInvalidState.Add(EBodySide.RIGHT, false);
    }

    // Read - only Propertise
    public Dictionary<EBodySide, TwoBoneIKConstraint> IkConstraint => _ikConstraint;
    public Dictionary<EBodySide, Transform> LegTransform => _legTransform;//hip
    public Dictionary<EBodySide, Transform> TargetTransform => _targetTransform;
    public Rigidbody Rb => _rb;
    public Collider RootCollider => _rootCollider;
    public Transform RootTransform => _rootTransform;
    public MainRagdollHandeler Mr => _mr;
    public float MaxStrideDisBAC => _maxStrideDisBAC;
    public float DifStrideDisBAC => _difStrideDisBAC;
    public float DifStrideDisFWD => _difStrideDisFWD;
    public float MinStrideDisFWD => _minStrideDisFWD;
    public float StrideDisFallVel => _strideDisFallVel;
    public float MaxStepDownDis => _maxStepDownDis;
    public float PlaceOffsetDis => _placeOffsetDis;
    public float ResetDur => _resetDur;
    public float ResetDurMod => _resetDurMod;
    public float IkEnterDur => _ikEnterDur;
    public float IkExitDur => _ikExitDur;
    public float MinActivePointDistance =>_minActivePointDistance;

    //Set-able variables
    public Dictionary<EBodySide, bool> OppositeInvalidState = new Dictionary<EBodySide, bool>();
    public bool IsMoving()
    {
        return Rb.linearVelocity.magnitude > 0.2f;
    }
    /*
    public void SetCurrentSide(Vector3 positionToCheck) // #
    {
        //compare distance from positionToCheck to leg positions
        Vector3 leftLeg = _leftIkConstraint.data.root.position;
        Vector3 rightLeg = _rightIkConstraint.data.root.position;

        //further leg is sided
        bool isFurther = Vector3.Distance(leftLeg, positionToCheck) > Vector3.Distance(rightLeg, positionToCheck); //# change to support still -> moving
        if (isFurther)
        {
            CurrentIkConstraint = _leftIkConstraint;
        }
        else
        {
            CurrentIkConstraint = _rightIkConstraint;
        }

        CurrentLegTransform = CurrentIkConstraint.data.root.transform;
        CurrentIkTargetTransform = CurrentIkConstraint.data.target.transform;
    }
    */

}
