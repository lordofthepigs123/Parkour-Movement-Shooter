using UnityEngine;
using thisEState = LegStateMachine.ELegState; // shorthands
using EEnviroment = EnviromentInteractionStateMachine.EEnviromentInteractionState;

public class LegBackStepState : LegState
{
    public LegBackStepState(LegContext lContext, thisEState estate) : base(lContext, estate)
    {

    }

    public override void EnterState()
    {
        Co.LastStepDir = EnviromentInteractionContext.EStepDir.BACKWARD;
        Co.LastStepSide = LContext.Side;
        Co.StaticNormal[LContext.Side] = Vector3.zero;
        LContext.ThisOppositeInvalidState[LContext.Side] = true;
        LContext.ReferencePos = LContext.ThisIkConstraint.data.root.position; //save current root pos
        LContext.StartPos = LContext.LockedPosition; //save current target
        LContext.StartNormal = LContext.LockedRotation * Vector3.up;
        LContext.ActiveRatio = 0;
    }
    public override void ExitState()
    {
        //resets
        LContext.ThisOppositeInvalidState[LContext.Side] = false;
    }
    public override void UpdateState()
    {
        LContext.FindFootNormal(-Co.RootTransform.up);
        Co.CalculateStride();

        //active estimate of final landing step point
        FindIkStepPosition(Co.BackStride);

        //Adjust for player rotation
        LContext.RotationAdjust();

        //find and set next IK target
        FindNextIkStridePosition();
        SetIkTarget(LContext.StridePos, LContext.StrideRotation); //#
        HoldIkTarget();
    }
    public override void LateUpdateState(){}
    public override thisEState GetNextState()
    {
        if (Co.Eism.CurrentStateKey == EEnviroment.Air || LContext.StrideInAir)
        {
            WalkToAirExitChecks();
            if (Co.LastStepSide != LContext.Side)
            {
                return thisEState.AirJump;
            }
            return thisEState.AirSearch;
        }

        bool fullCycle = LContext.ActiveRatio > Co.MinCompleteRatio;
        bool hitStepPointValid = LContext.StepCol != null;
        if (fullCycle && hitStepPointValid)
        {
            //home in on contact position, frozen
            Vector3 footNormal = BlendDotPowVec(Co.StepNormal[LContext.Side], Co.RootTransform.up, Co.RayNormalFac);

            //lock in to final pos and rot
            LContext.StridePos = LContext.StepPos + (1 - Vector3.Dot(LContext.ThisLegNormal, footNormal)) * Co.FootLength * LContext.ThisLegNormal; // anti clip rise
            
            LContext.StrideRotation = Quaternion.FromToRotation(Vector3.up,footNormal) * Quaternion.FromToRotation(Vector3.forward, Vector3.ProjectOnPlane(Co.RootTransform.forward, Vector3.up));
            SetIkTarget(LContext.StridePos, LContext.StrideRotation); //#
            HoldIkTarget();
            return thisEState.Search;
        }

        //altarnative into air/jump

        return StateKey;
    }

    private void FindNextIkStridePosition()
    {
        //get ratio _ traveled dis : frontal stride
        float progressRatio = -LContext.DistanceFromCenterFlat(LContext.ReferencePos, LContext.Side) / Co.ToMoveDisStride;
        if (float.IsNaN(progressRatio) || Co.ToMoveDisStride < 0.001f)
            progressRatio = 1;
        progressRatio = Mathf.Clamp(progressRatio, 0 , 1);
        if (progressRatio > LContext.ActiveRatio) // initiate break if decreasing ratio
        {
            //Determine if current progressRatio speed is too fast compared to player velocity
            float approxDis = FlatVelocity().magnitude * Co.SpeedLimiterThreshold * Time.deltaTime; // distance allowed to travel before modifying
            float ratioDif = progressRatio - LContext.ActiveRatio;
            float ratioDis = ratioDif * Co.ToMoveDisStride;
            float distanceOvershoot = ratioDis - approxDis;
            if (distanceOvershoot <= 0)
            {
                LContext.ActiveRatio = progressRatio;
            }
            else
            { // clamp to removeovershoot (slows down speed)
                LContext.ActiveRatio += approxDis / Co.ToMoveDisStride;
            }
        }
        else
        { // legs break speed
            LContext.ActiveRatio = Mathf.Lerp(LContext.ActiveRatio, 1, Co.BreakMult * Time.deltaTime);
        }

        Vector3 transitionNormal = Vector3.Lerp(LContext.StartNormal, Co.StepNormal[LContext.Side], LContext.ActiveRatio).normalized; // Lerped normal between start surface and final
        Vector3 footNormal = BlendDotPowVec(transitionNormal, Co.RootTransform.up, Co.RayNormalFac);

        //calc animation graph forward stride progress horizontal
        float horizontalPosRatio = Co.StrideCurve.Evaluate(LContext.ActiveRatio);
        //calc animation graph height
        float normalMult = Co.StrideHeightCurve.Evaluate(1 - LContext.ActiveRatio);
        Vector3 normalAdd = Co.FootLiftMult * normalMult * footNormal + (1 - Vector3.Dot(LContext.ThisLegNormal, footNormal)) * Co.FootLength * LContext.ThisLegNormal; // anti clip rise;

        Vector3 toStepDif = LContext.StepPos - LContext.StartPos;
        //Debug.DrawRay(LContext.StepPos, Vector3.up * 2, Color.red);
        //Debug.DrawRay(LContext.startPos, Vector3.up * 2, Color.blue);
        LContext.StridePos = LContext.StartPos + toStepDif * horizontalPosRatio + normalAdd;
        //Debug.DrawRay(LContext.StridePos, Vector3.up * 2, Color.green);

        float footRotAngle = Co.FootRotCurve.Evaluate(1 - LContext.ActiveRatio) * 90 * Co.FootLiftMult; // scale rotation magnitude based on foot lift mult
        LContext.StrideRotation = Quaternion.AngleAxis(footRotAngle, Vector3.right) * Quaternion.FromToRotation(Vector3.up, footNormal) * Quaternion.FromToRotation(Vector3.forward, Vector3.ProjectOnPlane(Co.RootTransform.forward, Vector3.up));
    }
}
