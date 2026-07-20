using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Side = EnviromentInteractionContext.EBodySide;

public class LegContext
{
    private Side _side;
    private Side _otherSide;
    private TwoBoneIKConstraint _thisIkConstraint;
    private Transform _thisLegTransform;
    private Transform _thisTargetTransform;
    private float _legLength;
    public LegContext(EnviromentInteractionContext context, Side side, Side otherSide)
    {
        Context = context;

        _side = side;
        _otherSide = otherSide;
        _thisIkConstraint = Context.LegIkConstraint[Side];
        _thisLegTransform = Context.LegTransform[Side];
        _thisTargetTransform = Context.LegTargetTransform[Side];
        _legLength = CalculatelegLength();
        OriginalFootRot = _thisIkConstraint.data.tip.rotation;
    }
    //read only
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
    public bool StrideInAir;
    public Vector3 LockedPosition; //target postion on current frame
    public Quaternion LockedRotation;

    public float ActiveRatio;

    private float CalculatelegLength()
    {
        return (ThisIkConstraint.data.tip.position - ThisLegTransform.position).magnitude;
    }

    public void FindLegNormal()
    {
        RaycastHit temp = GetStepPointRaycast((LegLength + 0.1f) * -Context.RootTransform.up, ThisIkConstraint.data.tip.position);
        ThisLegNormal = temp.normal; // zero if no hit
        ThisLegPoint = temp.point; // zero if no hit
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
        Vector3 flatDis = Vector3.ProjectOnPlane(distance,ThisLegNormal);
        float sign = Vector3.Dot(velNormal, Vector3.Project(distance.normalized,velNormal).normalized);
        
        return flatDis.magnitude * sign;
    }

    public Vector3 rootOnGroundPosition()
    {
        //get floor directly below legs root
        RaycastHit temp = GetStepPointRaycast((LegLength + 0.1f) * -Context.Rb.transform.up, ThisIkConstraint.data.root.position);
        if (temp.normal != Vector3.zero)
            return temp.point;

        return Context.RootTransform.position; // return the tranforms position
    }
    
}
