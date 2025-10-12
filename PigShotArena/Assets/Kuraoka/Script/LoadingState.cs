using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
public class LoadingState : GameState
{
    private string nextSceneName;
    private GameState nextState;

    public LoadingState(GameStateMachine sm, string nextScene, GameState nextState)
        : base(sm)
    {
        this.nextSceneName = nextScene;
        this.nextState = nextState;
    }

    public override string GetSceneName() => "LoadingScene";

    public override void Enter()
    {
        stateMachine.StartCoroutine(PlayLoadingSequence());
    }

    private IEnumerator PlayLoadingSequence()
    {
        // �@ �O�V�[���t�F�[�h�A�E�g
        var fadeOutController = GameObject.FindObjectOfType<FadeController>();
        if (fadeOutController != null)
            yield return fadeOutController.FadeOut();

        // �A Loading�V�[���񓯊����[�h�i�J�ڑҋ@�j
        AsyncOperation op = SceneManager.LoadSceneAsync(nextSceneName);
        op.allowSceneActivation = false;

        // �B Loading���o�Đ�
        var controller = GameObject.FindObjectOfType<LoadingSceneController>();
        if (controller != null)
            yield return controller.PlayLoadingAnimation();

        // �C ���[�h�����ҋ@
        while (op.progress < 0.9f) yield return null;

        // �D ���V�[���ɐؑ�
        op.allowSceneActivation = true;
        yield return new WaitUntil(() => op.isDone);

        // �E ���̃X�e�[�g��
        stateMachine.ChangeState(nextState);
    }
}
