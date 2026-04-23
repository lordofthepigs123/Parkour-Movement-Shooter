using UnityEngine;
using UnityEngine.Animations.Rigging;

public class LegContext
{
    private EnviromentInteractionContext.EBodySide _side;
    private EnviromentInteractionContext.EBodySide _otherSide;
    private TwoBoneIKConstraint _thisIkConstraint;
    private Transform _thisLegTransform;
    private Transform _thisTargetTransform;
    private bool _thisInvalidState;
    private float _legLength;
    public LegContext(EnviromentInteractionContext context, EnviromentInteractionContext.EBodySide side, EnviromentInteractionContext.EBodySide otherSide)
    {
        Context = context;

        _side = side;
        _otherSide = otherSide;
        _thisIkConstraint = Context.IkConstraint[Side];
        _thisLegTransform = Context.LegTransform[Side];
        _thisTargetTransform = Context.TargetTransform[Side];
        _thisInvalidState = Context.OppositeInvalidState[OtherSide];
        _legLength = CalculatelegLength();

        ThisOppositeInvalidState = Context.OppositeInvalidState[Side];
    }
    //read only
    public EnviromentInteractionContext.EBodySide Side => _side; // # public?
    public EnviromentInteractionContext.EBodySide OtherSide => _otherSide;
    public TwoBoneIKConstraint ThisIkConstraint => _thisIkConstraint;
    public Transform ThisLegTransform => _thisLegTransform;//hip
    public Transform ThisTargetTransform => _thisTargetTransform;
    public Vector3 ThisLegNormal {get; private set;}
    public bool ThisInvalidState => _thisInvalidState;
    public float LegLength => _legLength;
    //Set-able 
    public EnviromentInteractionContext Context {get; private set;}
    public bool ThisOppositeInvalidState;
    public RaycastHit StepHit;
    public bool StrideInAir;
    public RaycastHit FinalDestination;

    private float CalculatelegLength()
    {
        return (ThisIkConstraint.data.tip.position - ThisLegTransform.position).magnitude;
    }

    public void FindLegNormal()
    {
        ThisLegNormal = GetStepPointRaycast((Context.PlaceOffsetDis + 0.1f) * -Context.Rb.transform.up, ThisIkConstraint.data.tip.position).normal;
    }

    public RaycastHit GetStepPointRaycast(Vector3 checkDirLength, Vector3 checkPosition) //single foot ray check
    {
        RaycastHit pointHit;
        Physics.Raycast(checkPosition, checkDirLength, out pointHit);
        return pointHit;
    }

    
    public float DistanceFromCenterFlat()
    {
        //behind is negative, infront positive
        Vector3 velNormal = Vector3.ProjectOnPlane(Context.Rb.linearVelocity, ThisLegNormal).normalized;
        Vector3 distance = ThisIkConstraint.data.tip.position - ThisIkConstraint.data.root.position;
        distance = Vector3.Project(distance,velNormal);
        return distance.magnitude * Vector3.Dot(velNormal, distance.normalized);
    }

    public float ActivePointDistance()
    {
        Vector3 distance = StepHit.point - ThisIkConstraint.data.tip.position;
        return distance.magnitude;
    }
    
}
