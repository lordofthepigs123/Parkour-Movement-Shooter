using UnityEngine;
using thisEState = LegStateMachine.ELegState; // shorthands
using EEnviroment = EnviromentInteractionStateMachine.EEnviromentInteractionState;

public class LegSearchState : LegState
{
    private float resetTimer;
    public LegSearchState(LegContext lContext, thisEState estate) : base(lContext, estate)
    {

    }

    public override void EnterState()
    {
        Co.StaticNormal[LContext.Side] = Co.StepNormal[LContext.Side];
        resetTimer = Co.ResetDur;
    }
    public override void ExitState(){}
    public override void UpdateState()
    {
        if (resetTimer > 0)
        {
            resetTimer -= Time.deltaTime * Mathf.Pow(Co.ResetDurMod, Co.Rb.linearVelocity.magnitude);
        }

        LContext.FindFootNormal(-Co.RootTransform.up);
        Co.CalculateStride();

        //set IK target
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

        float displace = LContext.DistanceFromCenterFlat(LContext.TranslateAdjustOnNormal(LContext.ThisIkConstraint.data.tip.position), LContext.Side);
        float angDis = Vector3.Angle(LContext.LockedRotation * Vector3.forward, Vector3.ProjectOnPlane(Co.RootTransform.forward, LContext.LockedRotation * Vector3.up));
        float otherDisplace = LContext.DistanceFromCenterFlat(LContext.TranslateAdjustOnNormal(Co.LegIkConstraint[LContext.OtherSide].data.tip.position), LContext.OtherSide);
        float faceDirVelDot = Vector3.Dot(Vector3.ProjectOnPlane(Co.RootTransform.forward, CurrentNormal).normalized, FlatVelocity().normalized);
        bool steppingFwd = faceDirVelDot >= Co.StepDirThresholdBuf * (Co.LastStepDir == EnviromentInteractionContext.EStepDir.BACKWARD ? 1 : -1); // Step fwd or back, with margin bias towards last direction
        
        bool strideDisPassed, maxAnglePassed, significantDis, hitStepPointValid, otherFootValidPosition, overStreched, otherFootForward, otherFootFired, reseted, reverse;
        //active estimate of final landing step point
        if (steppingFwd)
        {
            FindIkStepPosition(Co.FrontalStride);
            strideDisPassed = displace < -Co.BackStride;
            otherFootForward = otherDisplace > 0;

            reverse = displace > Co.FrontalStride;
            bool slow = FlatVelocity().magnitude < Co.MaxReverseSpeed;
            if (reverse && slow) // reverse when overshot and going slowly
            {
                steppingFwd = false;
                strideDisPassed = true;
            }
        }
        else
        {
            FindIkStepPosition(Co.BackStride);
            strideDisPassed = displace < -Co.FrontalStride;
            otherFootForward = otherDisplace > Co.BackStride / Co.BackRunDivisor;

            reverse = displace > Co.BackStride;
            bool slow = FlatVelocity().magnitude < Co.MaxReverseSpeed;
            if (reverse && slow) // reverse when overshot and going slowly
            {
                steppingFwd = true;
                strideDisPassed = true;
            }
        }
        
        maxAnglePassed = angDis > Co.MaxAngleChange; // yaw angle significant, shuffle
        significantDis = -displace > Co.MinCenterDisplacement || (reverse && displace > Co.MinCenterDisplacement);
        hitStepPointValid = LContext.StepCol != null;
        otherFootFired = Co.LastStepSide != LContext.Side; // in alternating order
        reseted = resetTimer <= 0; // or reset time has passed
        otherFootValidPosition = !LContext.ThisOppositeInvalidState[LContext.OtherSide]; // walking
        overStreched = (LContext.ThisIkConstraint.data.tip.position - LContext.LockedPosition).magnitude > Co.StrechGive; // vs running 

        // placed foot has passed stride dis from center || placed foot yaw angle sig differs from current
        // FindIkStepPosition raycasts hit
        // Other foot fired before this one || enough reset time from last stride
        // Wait till other foot's stride complete || this foot is overStreched and other foot somewhat below or past center
        bool conditions = ((strideDisPassed && significantDis) || maxAnglePassed) && hitStepPointValid && (otherFootFired || reseted) && (otherFootValidPosition || (overStreched && otherFootForward));
        if (steppingFwd && conditions)
        {
            //Debug.Log("Search -> Step");
            return thisEState.Step;
        }

        if (conditions)
        {
            //Debug.Log("Search -> BackStep");
            return thisEState.BackStep;
        }
        
        LContext.ThisOppositeInvalidState[LContext.Side] = false; //redund

        return StateKey;
    }        
    
} 