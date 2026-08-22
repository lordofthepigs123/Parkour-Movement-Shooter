using System;
using UnityEngine;

using thisEState = EnviromentInteractionStateMachine.EEnviromentInteractionState; // shorthand

public class WalkState : EnviromentInteractionState
{
    public WalkState(EnviromentInteractionContext context, thisEState estate) : base(context, estate)
    {

    }

    public override void EnterState(){}
    public override void ExitState(){}
    public override void UpdateState()
    {
        Context.GetPlayerNormal();
        CalculateStrideHipHeight();

        SetHipTarget(Context.HipPos, Context.HipNormal);
        HoldHipTarget();
    }
    public override void LateUpdateState()
    {
        //save forward
        Context.SaveLastRootDir();
    }
    public override thisEState GetNextState()
    {
        if (Context.Sm.CurrentStateKey == PlayerStateMachine.EMovementState.air)
        {
            //when ungrounding or jumping
            return thisEState.Air;
        }

        return StateKey;
    }

    private void CalculateStrideHipHeight()
    {
        //lower height based on foot distance cycle and speed
        Vector3 targetDif = Context.LegTargetTransform[EnviromentInteractionContext.EBodySide.LEFT].position - Context.LegTargetTransform[EnviromentInteractionContext.EBodySide.RIGHT].position;
        targetDif = Vector3.ProjectOnPlane(targetDif, Context.SmoothPlayerNormal);
        float disRatio = targetDif.magnitude / (Context.FrontalStride + Context.BackStride);
        disRatio = Mathf.Clamp(disRatio, 0, 1);
        float normalMult = Context.HipDisToHeight.Evaluate(disRatio);
        //lower height on slopes
        float slopeMult = 0;
        if (Context.SmoothPlayerNormal != Vector3.zero) 
            slopeMult = Context.SlopeLowerMax * Mathf.Clamp(Vector3.Angle(Vector3.up, Context.SmoothPlayerNormal)/90,0,1);
        //Calculate hip 'Compersions' / 'Expansions' as a result of impulse forces
        Vector3 accelVert = -Vector3.Project(Context.Mr.FixedRbNetAccel, Context.RootTransform.up);
        Vector3 accelOff = -(Context.Mr.FixedRbNetAccel + accelVert);
        //MaxHipFlexVert MaxHipFlexOff HipFlexMod
        float multVert = Context.MaxHipFlexVert * (1 - (float)Math.Pow(Math.E, -accelVert.magnitude / Context.HipFlexMod)); // Max (1 - e^-x/a)
        float multOff = Context.MaxHipFlexOff * (1 - (float)Math.Pow(Math.E, -accelOff.magnitude / Context.HipFlexMod));
        if (float.IsNaN(multVert))
            accelVert = Vector3.zero;
        else
            accelVert = accelVert.normalized * multVert;

        if (float.IsNaN(multOff))
            accelOff = Vector3.zero;
        else
            accelOff = accelOff.normalized * multOff;

        //set desi position
        float offMult = normalMult * Context.FootLiftMult - slopeMult;
        Vector3 tBounceOffset;

        if (float.IsNaN(offMult))
            tBounceOffset = Vector3.zero;
        else
            tBounceOffset = Context.RootTransform.up * offMult;
        Context.BounceOffset = Vector3.Lerp(Context.BounceOffset, tBounceOffset, Context.HipBounceSmooth * Time.deltaTime);


        Vector3 tAccelOffset = accelVert + accelOff;
        float difference = (Context.HipPos - tAccelOffset).magnitude;
        if (difference > Context.LastDif)
            Context.LerpOffset = tAccelOffset;
        else
            Context.LerpOffset = Vector3.Lerp(Context.LerpOffset, tAccelOffset, Context.HipLerpSmooth * Time.deltaTime);
            
        Context.AccelOffset = Vector3.Lerp(Context.AccelOffset, Context.LerpOffset, Context.HipPosSmooth * Time.deltaTime * Mathf.Clamp(difference, 0.2f, Mathf.Infinity));
        Context.HipPos = Context.AccelOffset + Context.BounceOffset;
        Context.HipNormal = Context.RootTransform.up; //# currently simple always match transform

        //save for next
        Context.LastDif = difference;
    }
}
