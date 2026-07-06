using UnityEngine;
using DG.Tweening;
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
            CurrentNormal = Co.Rb.transform.up;
        }
        hitFromRoot = default;
        return false;
    }

    protected void ResetIkTargetPositionTracking()
    {
        LContext.StepPos = Vector3.zero;
        LContext.StepNormal = Vector3.zero;
        LContext.StepCol = null;
        LContext.LockedPosition = Vector3.zero;
        LContext.LockedRotation = LContext.OriginalFootRot;
    }

    private Vector3 CalculateStepRaycastDirLength()
    {
        Vector3 rayDirLength = -CurrentNormal;
        rayDirLength *= LContext.WaistToDownDist; //from waist to largest step down distance
        return rayDirLength;
    }
    
    private Vector3 CalculateStepRaycastPosition(float infront, Vector3 fwdDir)
    {
        Vector3 rayCastPos = Constraint.data.root.position + fwdDir * infront;
        return rayCastPos;
    }

    protected void CalculateStride()
    {
        //as speed increases overall stride length increase
        //but frontal(infront of player) stride decreases
        LContext.FrontalStride = Mathf.Pow(Co.StrideDisFallVel, -FlatVelocity().magnitude) * Co.DifStrideDisFWD + Co.MinStrideDisFWD; // s = d / (1+2/k)^x + m
        //and back(bahind player) stride increases
        LContext.BackStride = Co.MaxStrideDisBAC - Mathf.Pow(Co.StrideDisFallVel, -Velocity.magnitude) * Co.DifStrideDisBAC; // s = m - d / (1+2/k)^x
    }

    private Vector3 FlatVelocity()
    {
        return Vector3.ProjectOnPlane(Velocity, CurrentNormal);
    }

    protected void FindIkStepPosition()
    {
        //set below player position #move to Search state

        RaycastHit hitFromRoot;
        RaycastHit hitWall;
        if (!CheckInfrontWall(LContext.FrontalStride, out hitWall))
        {
            //Search IK position
            hitFromRoot = LContext.GetStepPointRaycast(CalculateStepRaycastDirLength(), CalculateStepRaycastPosition(LContext.FrontalStride, FlatVelocity().normalized));
            if (hitFromRoot.collider != null)
            {
                HitOffsetDecompose(hitFromRoot);
                LContext.StrideInAir = true;
                
                // ledge/air behaviour check #
                return;
            }
        }
        else
        {
            hitFromRoot = hitWall;
        }
        LContext.StrideInAir = false;

        //wall infront of desired stride forward location so stride up wall
        HitOffsetDecompose(hitFromRoot);
    }

    protected void SetIkTarget(Vector3 position, Vector3 normal)
    {
        //LContext.ThisLegTransform.position
        LContext.LockedPosition = position;
        LContext.LockedRotation = Quaternion.FromToRotation(Vector3.up, normal) * LContext.OriginalFootRot;
    }

    protected void HoldIkTarget()
    {
        LContext.ThisTargetTransform.position = LContext.LockedPosition;
        LContext.ThisTargetTransform.localRotation = LContext.LockedRotation;
    }

    private void HitOffsetDecompose(RaycastHit rayHit)
    {
        LContext.StepPos = rayHit.point + rayHit.normal * Co.PlaceOffsetDis;//away from surface, clipping
        LContext.StepNormal = rayHit.normal;
        LContext.StepCol = rayHit.collider;
    }

}
