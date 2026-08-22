using UnityEngine;
using UnityEngine.Animations.Rigging;

public abstract class LegState : BaseState<LegStateMachine.ELegState>
{
    //The Side variable of class instance is set as = LContext.Side
    protected LegContext LContext;
    protected EnviromentInteractionContext Co;//shorthand
    protected TwoBoneIKConstraint Constraint;
    protected Vector3 ColNormal => LContext.ThisLegNormal;
    protected Vector3 Velocity => Co.Rb.linearVelocity;
    protected Vector3 CurrentNormal;


    public LegState(LegContext lContext, LegStateMachine.ELegState stateKey) : base(stateKey)
    {
        LContext = lContext;
        //setup
        Co = LContext.Context;//shorthand
        Constraint = LContext.ThisIkConstraint;
        CurrentNormal = ColNormal;
    }

    protected bool CheckInfrontWall(float frontalStride, out RaycastHit hitFromRoot)
    {
        //Gets walk reference normal
        //Check infront of leg stride for obstacle
        CurrentNormal = ColNormal;
        RaycastHit frontalCheck = LContext.GetStepPointRaycast(FlatVelocity().normalized * (LContext.LegLength - frontalStride), Constraint.data.root.position);
        if (frontalCheck.collider != null)
        {//wall infront - change normal reference
            hitFromRoot = frontalCheck; // wall hit position is IK position #
            CurrentNormal = frontalCheck.normal;
            return true;
        }
        if (CurrentNormal.magnitude == 0)
        {
            CurrentNormal = Co.RootTransform.up;
        }
        hitFromRoot = default;
        return false;
    }

    protected void ResetIkTargetPositionTracking()
    {
        LContext.StepPos = Vector3.zero;
        Co.StepNormal[LContext.Side] = Vector3.zero;
        LContext.StepCol = null;
        LContext.LockedPosition = Vector3.zero;
        LContext.LockedRotation = LContext.OriginalFootRot;
    }

    private Vector3 CalculateStepRaycastDirLength()
    {
        //blend slope normal (high speed) and up vector
        LContext.rayDirNormal = -Vector3.Lerp(Vector3.up + CurrentNormal * Co.NormalMinFac, CurrentNormal, Co.SpeedStrideRatio).normalized;
        Vector3 rayDirLength = LContext.rayDirNormal * LContext.WaistToDownDist; //from waist to largest step down distance
        return rayDirLength;
    }
    
    private Vector3 CalculateStepRaycastPosition(float infront, Vector3 fwdDir)
    {
        Vector3 rayCastPos = Constraint.data.root.position + fwdDir * infront;
        return rayCastPos;
    }

    protected Vector3 FlatVelocity()
    {
        return Vector3.ProjectOnPlane(Velocity, CurrentNormal);
    }

    protected void FindIkStepPosition(float inFront)
    {
        RaycastHit hitFromRoot;
        RaycastHit hitWall;
        if (!CheckInfrontWall(inFront, out hitWall))
        {
            //Search IK position
            hitFromRoot = LContext.GetStepPointRaycast(CalculateStepRaycastDirLength(), CalculateStepRaycastPosition(inFront, FlatVelocity().normalized));
            if (hitFromRoot.collider == null)
            {
                LContext.StrideInAir = true;
                //when no hits
                // ledge/air behaviour check #
                return;
            }
            //when step hit
        }
        else
        {
            hitFromRoot = hitWall;
        }
        LContext.StrideInAir = false;

        //wall infront of desired stride forward location so stride up wall
        HitOffsetDecompose(hitFromRoot);
    }

    protected void SetIkTarget(Vector3 position, Quaternion rotation)
    {
        //LContext.ThisLegTransform.position
        LContext.LockedPosition = position;
        LContext.LockedRotation = rotation;
    }

    protected void HoldIkTarget()
    {
        LContext.ThisTargetTransform.position = LContext.LockedPosition;
        LContext.ThisTargetTransform.rotation = LContext.LockedRotation * LContext.OriginalFootRot;
    }

    private void HitOffsetDecompose(RaycastHit rayHit)
    {
        LContext.StepPos = rayHit.point + rayHit.normal * Co.PlaceOffsetDis;//away from surface, clipping
        Co.StepNormal[LContext.Side] = rayHit.normal;
        LContext.StepCol = rayHit.collider;
    }
    protected void SetFDetectorEnabled(bool enabled)
    {
        Co.Fd.Fdh.StateHasEnabled = enabled;
    }

    protected void AirToWalkExitPrep()
    {
        LContext.FindLegNormal();
        Co.CalculateStride();
        FindIkStepPosition(Co.FrontalStride);
    }

    protected void AirToWalkExitChecks()
    {
        if (LContext.ThisLegNormal != Vector3.zero)
            {
                LContext.StridePos = LContext.ThisLegPoint + LContext.ThisLegNormal * Co.PlaceOffsetDis;
                LContext.StrideRotation = Quaternion.FromToRotation(Vector3.up,LContext.ThisLegNormal) * Quaternion.FromToRotation(Vector3.forward, Vector3.ProjectOnPlane(Co.RootTransform.forward, Vector3.up));
            }
        SetIkTarget(LContext.StridePos, LContext.StrideRotation);
        LContext.AirFrontLeg = LegContext.FrontLeg.UNSET;
    }

    protected void WalkToAirExitChecks()
    {
        
    }
}
