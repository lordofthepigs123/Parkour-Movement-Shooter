using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Side = EnviromentInteractionContext.EBodySide;

public class LegContext
{
    private LegStateMachine _lsm;
    private Side _side;
    private Side _otherSide;
    private TwoBoneIKConstraint _thisIkConstraint;
    private Transform _thisLegTransform;
    private Transform _thisTargetTransform;
    private float _legLength;
    public LegContext(LegStateMachine lsm, EnviromentInteractionContext context, Side side, Side otherSide)
    {
        _lsm = lsm;
        Context = context;

        _side = side;
        _otherSide = otherSide;
        _thisIkConstraint = Context.LegIkConstraint[Side];
        _thisLegTransform = Context.LegTransform[Side];
        _thisTargetTransform = Context.LegTargetTransform[Side];
        _legLength = CalculatelegLength();
        Context.LegLength = _legLength;
        OriginalFootRot = _thisIkConstraint.data.tip.rotation;
    }
    //read only
    public LegStateMachine Lsm => _lsm;
    public Side Side => _side;
    public Side OtherSide => _otherSide;
    public TwoBoneIKConstraint ThisIkConstraint => _thisIkConstraint;
    public Transform ThisLegTransform => _thisLegTransform;//hip
    public Transform ThisTargetTransform => _thisTargetTransform;
    public Vector3 ThisLegNormal {get; private set;} // normal of ground below current foot
    public Vector3 ThisLegPoint {get; private set;} // point of hit below foot
    public Quaternion OriginalFootRot {get; private set;}
    public float LegLength => _legLength;
    public float WaistToDownDist => LegLength + Context.MaxStepDownDis; //from waist to largest step down distance

    //Set-able 
    public EnviromentInteractionContext Context {get; private set;}
    public Dictionary<Side, bool> ThisOppositeInvalidState => Context.OppositeInvalidState;
    public Vector3 StepPos; //current estimate final pos
    public Collider StepCol;
    public Vector3 StridePos; //lerped position based on pos body og vs current / fwd stride
    public Quaternion StrideRotation;
    public Vector3 referencePos; //body position at stride start
    public Vector3 startPos; //foot position at stride start
    public Vector3 startNormal; //foot position at stride start
    public Vector3 rayDirNormal; //direction used for raycast check step position
    public bool StrideInAir;
    public Vector3 LockedPosition; //target postion on current frame
    public Quaternion LockedRotation;
    public enum FrontLeg
    {
        TRUE,
        FLASE,
        UNSET
    }
    public FrontLeg AirFrontLeg;
    public Vector3 LocalAirPos; //Position of target while in air that tracks body
    public Vector3 OGLocalAirPos;

    public float ActiveRatio;

    private float CalculatelegLength()
    {
        return (ThisIkConstraint.data.tip.position - ThisLegTransform.position).magnitude;
    }

    public void FindLegNormal()
    {
        RaycastHit temp = GetStepPointRaycast(WaistToDownDist * -Context.RootTransform.up, ThisIkConstraint.data.tip.position);
        //Debug.DrawRay(ThisIkConstraint.data.tip.position, WaistToDownDist * -Context.RootTransform.up, Color.rebeccaPurple);
        ThisLegNormal = temp.normal; // zero if no hit
        ThisLegPoint = temp.point; // zero if no hit
    }

    public Vector3 RootToGround()
    {
        RaycastHit temp = GetStepPointRaycast(WaistToDownDist * rayDirNormal, ThisIkConstraint.data.root.position);
        Vector3 tempDif = temp.point - ThisIkConstraint.data.root.position;
        return tempDif;
    }

    public RaycastHit GetStepPointRaycast(Vector3 checkDirLength, Vector3 checkPosition) //single foot ray check
    {
        RaycastHit pointHit;
        Physics.Raycast(checkPosition, checkDirLength, out pointHit, checkDirLength.magnitude, Context.GroundLayer);
        return pointHit;
    }

    
    public float DistanceFromCenterFlat(Vector3 comparePoint, Side ebodySide)
    {
        //behind is negative, infront positive
        Vector3 velNormal = Vector3.ProjectOnPlane(Context.Rb.linearVelocity + Context.RootTransform.forward * 0.1f, ThisLegNormal).normalized;
        Vector3 distance = comparePoint - Context.LegIkConstraint[ebodySide].data.root.position;
        Vector3 flatDis = Vector3.ProjectOnPlane(distance, ThisLegNormal);
        float sign = Vector3.Dot(velNormal, Vector3.Project(distance.normalized,velNormal).normalized);
        return flatDis.magnitude * sign;
    }

    public Vector3 TranslateAdjustOnNormal(Vector3 point)
    {
        Vector3 rootToground = RootToGround();
        float targetMag = Mathf.Cos(Vector3.Angle(rootToground, -ThisLegNormal)) * rootToground.magnitude; // a = cos() * h
        Vector3 target = ThisIkConstraint.data.root.position - targetMag * ThisLegNormal; // calc third point on right triangle
        Vector3 hitToTarget = target - (ThisIkConstraint.data.root.position + rootToground); // calc translation along ground plane
        return point + hitToTarget; // apply translation
    }

    public void RotationAdjust()
    {
        // move the set reference positions for start of stride when turning camera to remain behind camera
        Quaternion adjustRot = Quaternion.FromToRotation(Vector3.ProjectOnPlane(Context.LastRootDir, startNormal), Vector3.ProjectOnPlane(Context.RootTransform.forward, startNormal));
        referencePos = RotateAroundPoint(referencePos, Context.RootTransform.position, adjustRot);
        startPos = RotateAroundPoint(startPos, Context.RootTransform.position, adjustRot);
    }

    public Vector3 RotateAroundPoint(Vector3 point, Vector3 pivot, Quaternion rotation)
    {
        Vector3 direction = point - pivot; 
        Vector3 rotatedDirection = rotation * direction; 
    
        return pivot + rotatedDirection; 
    }
    
}
