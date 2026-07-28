using UnityEngine;
using thisEState = EnviromentInteractionStateMachine.EEnviromentInteractionState; // shorthand

public class AirState : EnviromentInteractionState
{
    public AirState(EnviromentInteractionContext context, thisEState estate) : base(context, estate)
    {

    }

    public override void EnterState(){}
    public override void ExitState(){}
    public override void UpdateState()
    {
        //Context.GetPlayerNormal();
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
            Debug.Log("Air -> Walk");
            return thisEState.Walk;
        }

        return StateKey;
    }


}
