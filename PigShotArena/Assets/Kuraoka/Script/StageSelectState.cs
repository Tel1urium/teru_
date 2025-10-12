using UnityEngine;

public class StageSelectState : GameState
{
    public StageSelectState(GameStateMachine sm) : base(sm) { }
    public override string GetSceneName() => "StageSelectScene";

    public override void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            stateMachine.ChangeState(
                new LoadingState(stateMachine, "Normal", new StageState(stateMachine, 1))
            );
        if (Input.GetKeyDown(KeyCode.Alpha2))
            stateMachine.ChangeState(
                new LoadingState(stateMachine, "Ice", new StageState(stateMachine, 2))
            );
        if (Input.GetKeyDown(KeyCode.Alpha3))
            stateMachine.ChangeState(
                new LoadingState(stateMachine, "trampoline", new StageState(stateMachine, 3))
            );
    }
}
