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
    public override thisEState GetNextState()
    {
        return StateKey;
    }

    private void CalculateStrideHipHeight()
    {
        Vector3 targetDif = Context.LegTargetTransform[EnviromentInteractionContext.EBodySide.LEFT].position - Context.LegTargetTransform[EnviromentInteractionContext.EBodySide.RIGHT].position;
        targetDif = Vector3.ProjectOnPlane(targetDif, Context.SmoothPlayerNormal);
        float disRatio = targetDif.magnitude / (Context.FrontalStride + Context.BackStride);
        disRatio = Mathf.Clamp(disRatio, 0, 1);
        float normalMult = Context.HipDisToHeight.Evaluate(disRatio);
        Vector3 offset = Context.RootTransform.up * normalMult * Context.FootLiftMult;
        Context.HipPos = Vector3.Lerp(Context.HipPos, offset, Context.HipBounceSmooth * Time.deltaTime);
        Context.HipNormal = Context.RootTransform.up; //# currently simple always match transform
    }
}
