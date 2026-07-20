using UnityEngine;

using thisEState = LegStateMachine.ELegState; // shorthand

public class LegSearchState : LegState
{
    private float resetTimer;
    public LegSearchState(LegContext lContext, thisEState estate) : base(lContext, estate)
    {

    }

    public override void EnterState()
    {
        resetTimer = Co.ResetDur;
    }
    public override void ExitState(){}
    public override void UpdateState()
    {
        if (resetTimer > 0)
        {
            resetTimer -= Time.deltaTime * Mathf.Pow(Co.ResetDurMod, Co.Rb.linearVelocity.magnitude);
        }

        LContext.FindLegNormal();//#
        Co.CalculateStride();

        //set IK target
        HoldIkTarget();
    }
    public override thisEState GetNextState()
    {
        float displace = LContext.DistanceFromCenterFlat(LContext.ThisIkConstraint.data.tip.position, LContext.Side);
        float angDis = Vector3.Angle(LContext.LockedRotation * Vector3.forward, Vector3.ProjectOnPlane(Co.RootTransform.forward, LContext.LockedRotation * Vector3.up));
        Debug.Log(angDis + " h");
        float otherDisplace = LContext.DistanceFromCenterFlat(Co.LegIkConstraint[LContext.OtherSide].data.tip.position, LContext.OtherSide);
        float faceDirVelDot = Vector3.Dot(Vector3.ProjectOnPlane(Co.RootTransform.forward, CurrentNormal).normalized, FlatVelocity().normalized);
        bool steppingFwd = faceDirVelDot >= Co.StepDirThresholdBuf * (Co.LastStepDir == EnviromentInteractionContext.EStepDir.BACKWARD ? 1 : -1); // Step fwd or back, with margin bias towards last direction
        
        bool strideDisPassed, maxAnglePassed, significantDis, hitStepPointValid, otherFootValidPosition, overStreched, otherFootForward, otherFootFired, reseted;

        //active estimate of final landing step point
        if (steppingFwd)
        {
            FindIkStepPosition(Co.FrontalStride);
            strideDisPassed = displace < -Co.BackStride;
            otherFootForward = otherDisplace > 0;
        }
        else
        {
            FindIkStepPosition(Co.BackStride);
            strideDisPassed = displace < -Co.FrontalStride;
            otherFootForward = otherDisplace > Co.BackStride / Co.BackRunDivisor;
        }
        
        maxAnglePassed = angDis > Co.MaxAngleChange;
        significantDis = -displace > Co.MinCenterDisplacement;
        hitStepPointValid = LContext.StepCol != null;
        otherFootFired = Co.LastStepSide != LContext.Side; // in alternating order
        reseted = resetTimer <= 0; // or reset time has passed
        otherFootValidPosition = !LContext.ThisOppositeInvalidState[LContext.OtherSide]; // walking
        overStreched = (LContext.ThisIkConstraint.data.tip.position - LContext.LockedPosition).magnitude > Co.StrechGive; // vs running 
        bool conditions = ((strideDisPassed  && significantDis) || maxAnglePassed) && hitStepPointValid && (otherFootFired || reseted) && (otherFootValidPosition || (overStreched && otherFootForward));
        if (steppingFwd && conditions)
        {
            Debug.Log("Search -> Step");
            return thisEState.Step;
        }

        if (conditions)
        {
            Debug.Log("Search -> BackStep");
            return thisEState.BackStep;
        }
        
        LContext.ThisOppositeInvalidState[LContext.Side] = false; //redund

        return StateKey;
    }        
    
} 