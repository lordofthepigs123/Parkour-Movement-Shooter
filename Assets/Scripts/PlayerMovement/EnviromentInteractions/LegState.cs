using UnityEngine;
using UnityEngine.Animations.Rigging;

public abstract class LegState : BaseState<LegStateMachine.ELegState>
{
    //The Side variable of class instance is set as = LContext.Side
    protected LegContext LContext;
    protected EnviromentInteractionContext Co;//shorthand
    protected TwoBoneIKConstraint Constraint;
    protected Vector3 ColNormal;
    protected Vector3 CurrentNormal;
    protected Vector3 Velocity;


    public LegState(LegContext lContext, LegStateMachine.ELegState stateKey) : base(stateKey)
    {
        LContext = lContext;
        //setup
        Co = LContext.Context;//shorthand
        Constraint = Co.IkConstraint[LContext.Side];
        ColNormal = Co.LegNormal[LContext.Side];
        CurrentNormal = ColNormal;
        Velocity = Co.Rb.linearVelocity;
    }

    protected bool CheckInfrontWall(float frontalStride, RaycastHit hitFromRoot)
    {
        //Gets walk reference normal
        //Check infront of leg stride for obstacle
        RaycastHit frontalCheck = GetStepPointRaycast(FlatVelocity().normalized * frontalStride, Constraint.data.root.position);
        if (frontalCheck.collider != null)
        {//wall infront - change normal reference
            hitFromRoot = frontalCheck; // wall hit position is IK position #
            ColNormal = frontalCheck.normal;
            return true;
        }
        return false;
    }

    private RaycastHit GetStepPointRaycast(Vector3 checkDirLength, Vector3 checkPosition) //single foot ray check
    {
        RaycastHit pointHit;
        Physics.Raycast(checkPosition, checkDirLength, out pointHit);
        return pointHit;
    }

    //inheritable methods for affecting target IK
    protected void StartIkTargetPositionTracking(Collider intersectingCollider)
    {
        //set below player position #move to Search state
        float frontalStride;
        float backStride;
        (frontalStride, backStride) = CalculateStride();

        RaycastHit hitFromRoot = new RaycastHit();
        if (!CheckInfrontWall(frontalStride, hitFromRoot))
        {
            //Search IK position
            hitFromRoot = GetStepPointRaycast(CalculateStepRaycastDirLength(), CalculateStepRaycastPosition(frontalStride, FlatVelocity().normalized));

            if (hitFromRoot.collider != null)
            {
                // ledge/air behaviour check #
                return;
            }
        }

        LContext.StepHit = hitFromRoot;
    }
    protected void UpdateIkTargetPosition(Collider intersectingCollider)
    {
        
    }
    protected void ResetIkTargetPositionTracking(Collider intersectingCollider)
    {
        
    }

    protected Vector3 CalculateStepRaycastDirLength()
    {
        Vector3 rayDirLength = CurrentNormal;
        rayDirLength *= Co.LegLength + Co.MaxStepDownDis; //from waist to largest step down distance
        return rayDirLength;
    }
    
    protected Vector3 CalculateStepRaycastPosition(float infront, Vector3 fwdDir)
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

    protected Vector3 FlatVelocity()
    {
        return Vector3.ProjectOnPlane(Velocity, CurrentNormal);
    }
}
