using Unity.Loading;
using UnityEngine;

public class ResultState : GameState
{
    public ResultState(GameStateMachine sm) : base(sm) { }
    public override string GetSceneName() => "ResultScene";

    public override void Update()
    {
        if (Input.anyKeyDown)
        {
            stateMachine.ChangeState(
                new LoadingState(stateMachine, "TitleScene", new TitleState(stateMachine))
            );
        }
    }
}
