using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Side = EnviromentInteractionContext.EBodySide;

public class ArmContext
{
   private Side _side;
    private Side _otherSide;
    private TwoBoneIKConstraint _thisIkConstraint;
    private Transform _thisTargetTransform;
    public ArmContext(EnviromentInteractionContext context, Side side, Side otherSide)
    {
        Context = context;

        _side = side;
        _otherSide = otherSide;
        _thisIkConstraint = Context.ArmIkConstraint[Side];
        _thisTargetTransform = Context.ArmTargetTransform[Side];
        OriginalHandRot = _thisIkConstraint.data.tip.rotation;
    }
    //read only
    public Side Side => _side;
    public Side OtherSide => _otherSide;
    public TwoBoneIKConstraint ThisIkConstraint => _thisIkConstraint;
    public Transform ThisTargetTransform => _thisTargetTransform;
    public Quaternion OriginalHandRot {get; private set;}

    //Set-able 
    public EnviromentInteractionContext Context {get; private set;}
    public Vector3 LockedPosition; //target postion on current frame
    public Quaternion LockedRotation;
}
