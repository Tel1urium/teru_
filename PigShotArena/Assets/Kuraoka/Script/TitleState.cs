using Unity.Loading;
using UnityEngine;

public class TitleState : GameState
{
    public TitleState(GameStateMachine sm) : base(sm) { }
    public override string GetSceneName() => "TitleScene";

    // Update() 内のキー入力は不要にする
    public void OnStartButtonPressed()
    {
        // ボタン押下時に LoadingState に遷移
        stateMachine.ChangeState(
            new LoadingState(stateMachine, "StageSelect", new StageSelectState(stateMachine))
        );
    }
}
