using UnityEngine;
using thisEState = EnviromentInteractionStateMachine.EEnviromentInteractionState; // shorthand

public class AirState : EnviromentInteractionState
{
    public AirState(EnviromentInteractionContext context, thisEState estate) : base(context, estate)
    {

    }

    public override void EnterState(){}
    public override void ExitState()
    {
        ResetGeneralLegDir();
    }
    public override void UpdateState()
    {
        CalculateGeneralLegDir();
    }
    public override void LateUpdateState()
    {
        //save forward
        Context.SaveLastRootDir();
    }
    public override thisEState GetNextState()
    {
        if (Context.Sm.CurrentStateKey == PlayerStateMachine.EMovementState.walking)
        {
            //when regrounding to walk
            return thisEState.Walk;
        }

        return StateKey;
    }

    private void CalculateGeneralLegDir()
    {
        Vector3 leftlegDis = Context.LegIkConstraint[EnviromentInteractionContext.EBodySide.LEFT].data.tip.position - Context.LegIkConstraint[EnviromentInteractionContext.EBodySide.LEFT].data.root.position;
        Vector3 rightlegDis = Context.LegIkConstraint[EnviromentInteractionContext.EBodySide.RIGHT].data.tip.position - Context.LegIkConstraint[EnviromentInteractionContext.EBodySide.RIGHT].data.root.position;
        Vector3 generalDis = (leftlegDis + rightlegDis) / (2 * Context.LegLength);
        generalDis = generalDis.normalized * generalDis.sqrMagnitude;
        Context.Mr.GeneralLegDirection = generalDis;
        Debug.DrawRay(Context.RootTransform.position,Context.Mr.GeneralLegDirection,Color.red,2);
    }

    private void ResetGeneralLegDir()
    {
        Context.Mr.GeneralLegDirection = Vector3.zero;
    }
}
