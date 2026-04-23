using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.Rendering.LookDev;
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
            CurrentNormal = Co.Rb.transform.up;
        }
        hitFromRoot = default;
        return false;
    }

    protected void ResetIkTargetPositionTracking()
    {
        LContext.StepHit = new RaycastHit();
    }

    private Vector3 CalculateStepRaycastDirLength()
    {
        Vector3 rayDirLength = -CurrentNormal;
        rayDirLength *= LContext.LegLength + Co.MaxStepDownDis; //from waist to largest step down distance
        return rayDirLength;
    }
    
    private Vector3 CalculateStepRaycastPosition(float infront, Vector3 fwdDir)
    {
        Vector3 rayCastPos = Constraint.data.root.position + fwdDir * infront;
        return rayCastPos;
    }

    protected (float, float) CalculateStride()
    {
        //as speed increases overall stride length increase
        //but frontal(infront of player) stride decreases
        float frontalStride = Mathf.Pow(Co.StrideDisFallVel, -FlatVelocity().magnitude) * Co.DifStrideDisFWD + Co.MinStrideDisFWD; // s = d / (1+2/k)^x + m
        //and back(bahind player) stride increases
        float backStride = Co.MaxStrideDisBAC - Mathf.Pow(Co.StrideDisFallVel, -Velocity.magnitude) * Co.DifStrideDisBAC; // s = m - d / (1+2/k)^x
        //return front and back length
        return (frontalStride, backStride);
    }

    protected bool StrideDisPassed()
    {
        float frontalStride;
        float backStride;
        (frontalStride, backStride) = CalculateStride();

        return LContext.DistanceFromCenterFlat() < -backStride;
    }

    private Vector3 FlatVelocity()
    {
        return Vector3.ProjectOnPlane(Velocity, CurrentNormal);
    }

    protected void FindIkStepPosition()
    {
        //set below player position #move to Search state
        float frontalStride;
        float backStride;
        (frontalStride, backStride) = CalculateStride();

        RaycastHit hitFromRoot;
        if (!CheckInfrontWall(frontalStride, out hitFromRoot))
        {
            //Search IK position
            hitFromRoot = LContext.GetStepPointRaycast(CalculateStepRaycastDirLength(), CalculateStepRaycastPosition(frontalStride, FlatVelocity().normalized));

            if (hitFromRoot.collider != null)
            {
                LContext.StrideInAir = true;

                // ledge/air behaviour check #
                return;
            }
        }
        LContext.StrideInAir = false;

        LContext.StepHit = hitFromRoot;

    }

    private void OnDrawGizmos()
    {
        //Gizmos        
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(LContext.StepHit.point,0.03f);
    }

    protected void SetIkTarget(Vector3 position, Vector3 normal)
    {
        //LContext.ThisLegTransform.position
        Vector3 offsetPosition = position + LContext.StepHit.normal * Co.PlaceOffsetDis;//away from surface, clipping
        LContext.ThisTargetTransform.position = offsetPosition;
        LContext.ThisTargetTransform.localRotation = Quaternion.FromToRotation(Vector3.up, normal);
    }

}
