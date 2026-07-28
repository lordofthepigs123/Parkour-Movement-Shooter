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
            Debug.Log("Walk -> Air");
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
        //set desi position
        float offMult = normalMult * Context.FootLiftMult - slopeMult;
        Vector3 offset;

        if (float.IsNaN(offMult))
            offset = Vector3.zero;
        else
            offset = Context.RootTransform.up * offMult;

        float difference = (Context.HipPos - offset).magnitude;
        Context.HipPos = Vector3.Lerp(Context.HipPos, offset, Context.HipBounceSmooth * Time.deltaTime / Mathf.Pow(difference, 0.5f));
        Context.HipNormal = Context.RootTransform.up; //# currently simple always match transform
    }
}
