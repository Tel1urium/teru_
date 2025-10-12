using Unity.Loading;
using UnityEngine;

public class TitleState : GameState
{
    public TitleState(GameStateMachine sm) : base(sm) { }
    public override string GetSceneName() => "TitleScene";

    // Update() ���̃L�[���͕͂s�v�ɂ���
    public void OnStartButtonPressed()
    {
        // �{�^���������� LoadingState �ɑJ��
        stateMachine.ChangeState(
            new LoadingState(stateMachine, "StageSelect", new StageSelectState(stateMachine))
        );
    }
}
