using UnityEngine;
using thisEState = LegStateMachine.ELegState; // shorthands
using EEnviroment = EnviromentInteractionStateMachine.EEnviromentInteractionState;

public class LegStepState : LegState
{
    public LegStepState(LegContext lContext, thisEState estate) : base(lContext, estate)
    {

    }

    public override void EnterState()
    {
        Co.LastStepDir = EnviromentInteractionContext.EStepDir.FORWARD;
        Co.LastStepSide = LContext.Side;
        Co.StaticNormal[LContext.Side] = Vector3.zero;
        LContext.ThisOppositeInvalidState[LContext.Side] = true;
        LContext.referencePos = LContext.ThisIkConstraint.data.root.position; //save current root pos
        LContext.startPos = LContext.LockedPosition; //save current target
        LContext.startNormal = LContext.LockedRotation * Vector3.up;
        LContext.ActiveRatio = 0;
    }
    public override void ExitState()
    {
        //lock in to final pos and rot
        Co.StaticNormal[LContext.Side] = Co.StepNormal[LContext.Side];
        LContext.StridePos = LContext.StepPos;
        LContext.StrideRotation = Quaternion.FromToRotation(Vector3.up,Co.StepNormal[LContext.Side]) * Quaternion.FromToRotation(Vector3.forward, Vector3.ProjectOnPlane(Co.RootTransform.forward, Vector3.up));
        SetIkTarget(LContext.StridePos, LContext.StrideRotation); //#
        HoldIkTarget();

        //resets
        LContext.ThisOppositeInvalidState[LContext.Side] = false;
    }
    public override void UpdateState()
    {
        LContext.FindLegNormal();//#more robust
        Co.CalculateStride();

        //active estimate of final landing step point
        FindIkStepPosition(Co.FrontalStride);

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
        if (Co.Eism.CurrentStateKey ==  EEnviroment.Air)
        {
            //Debug.Log("Search -> AirSearch");
            return thisEState.AirSearch;
        }

        bool fullCycle = LContext.ActiveRatio > Co.MinCompleteRatio;
        bool hitStepPointValid = LContext.StepCol != null;
        if (fullCycle && hitStepPointValid)
        {
            //home in on contact position, frozen
            //Debug.Log("Step -> Search");
            return thisEState.Search;
        }

        //altarnative into air/jump

        return StateKey;
    }

    private void FindNextIkStridePosition()
    {
        //get ratio _ traveled dis : ToMoveDisStride
        float progressRatio = -LContext.DistanceFromCenterFlat(LContext.referencePos, LContext.Side) / Co.ToMoveDisStride;
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


        Vector3 transitionNormal = Vector3.Lerp(LContext.startNormal, Co.StepNormal[LContext.Side], LContext.ActiveRatio).normalized; // Lerped normal between start surface and final
        
        //calc animation graph forward stride progress horizontal
        float horizontalPosRatio = Co.StrideCurve.Evaluate(LContext.ActiveRatio);
        //calc animation graph height
        float normalMult = Co.StrideHeightCurve.Evaluate(LContext.ActiveRatio);
        Vector3 normalAdd = Co.FootLiftMult * normalMult * transitionNormal;

        Vector3 toStepDif = LContext.StepPos - LContext.startPos;
        //Debug.DrawRay(LContext.StepPos, Vector3.up * 2, Color.red);
        //Debug.DrawRay(LContext.startPos, Vector3.up * 2, Color.blue);
        LContext.StridePos = LContext.startPos + toStepDif * horizontalPosRatio + normalAdd;
        //Debug.DrawRay(LContext.StridePos, Vector3.up * 2, Color.green);

        float footRotAngle = Co.FootRotCurve.Evaluate(LContext.ActiveRatio) * 90 * Co.FootLiftMult; // scale rotation magnitude based on foot lift mult
        LContext.StrideRotation = Quaternion.AngleAxis(footRotAngle, Vector3.right) * Quaternion.FromToRotation(Vector3.up,transitionNormal) * Quaternion.FromToRotation(Vector3.forward, Vector3.ProjectOnPlane(Co.RootTransform.forward, Vector3.up));
    }
}
