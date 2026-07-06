using UnityEngine;
using thisEState = LegStateMachine.ELegState; // shorthand

public class LegStepState : LegState
{
    public LegStepState(LegContext lContext, thisEState estate) : base(lContext, estate)
    {

    }

    public override void EnterState()
    {
        LContext.ThisOppositeInvalidState[LContext.Side] = true;
        LContext.referencePos = LContext.ThisIkConstraint.data.root.position; //save current root pos
        LContext.startPos = LContext.LockedPosition; //save current target
    }
    public override void ExitState()
    {
        LContext.ThisOppositeInvalidState[LContext.Side] = false;
    }
    public override void UpdateState()
    {
        LContext.FindLegNormal();//#more robust
        CalculateStride();
        //other leg can't step until this toe past center
        LContext.ThisOppositeInvalidState[LContext.Side] = LContext.DistanceFromCenterFlat(LContext.ThisIkConstraint.data.tip.position) < 0;

        //active estimate of final landing step point
        FindIkStepPosition();

        //find and set next IK target
        FindNextIkStridePosition();
        SetIkTarget(LContext.StridePos, LContext.StrideNormal); //#
        HoldIkTarget();
    }
    public override thisEState GetNextState()
    {
        bool inHitProximity = LContext.ActivePointDistance() < Co.MinActivePointDistance;
        bool hitStepPointValid = LContext.StepCol != null;
        if (inHitProximity && hitStepPointValid)
        {
            //home in on contact position, frozen
            Debug.Log("step -> reset");
            return thisEState.Reset;
        }

        //altarnative into air/jump

        return StateKey;
    }

    private void FindNextIkStridePosition()
    {
        LContext.StrideNormal = LContext.StepNormal; //#

        //get ratio _ traveled dis : frontal stride
        float progressRatio = -LContext.DistanceFromCenterFlat(LContext.referencePos) / LContext.FrontalStride;
        progressRatio = Mathf.Clamp(progressRatio, 0 , 1);
        //(LContext.BackStride + LContext.DistanceFromCenterFlat()) / totalStride;

        //calc animation graph forward stride progress horizontal
        float horizontalPosRatio = Co.StrideCurve.Evaluate(progressRatio);
        //calc animation graph height
        float normalMult = Co.StrideHeightCurve.Evaluate(progressRatio);
        Vector3 normalAdd = LContext.StrideNormal * normalMult;
        //Debug.Log(progressRatio + "  " + horizontalPosRatio);
        //# Set locked as center for stop
        //LContext.StepPos - activly changing infornt raycast point
        //LContext.LockedPosition LContext.StridePos - this frame point of movement
        //LContext.referencePos - center position at time of start stride
        //LContext.ThisIkConstraint.data.root.position - center position now
        //Co.

        Vector3 toStepDif = LContext.StepPos - LContext.startPos;
        Debug.DrawRay(LContext.StepPos, Vector3.up * 2, Color.red);
        Debug.DrawRay(LContext.startPos, Vector3.up * 2, Color.blue);
        LContext.StridePos = LContext.startPos + toStepDif * horizontalPosRatio + normalAdd;
        Debug.DrawRay(LContext.StridePos, Vector3.up * 2, Color.green);
        Debug.Log(progressRatio);
    }
}
