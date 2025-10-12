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
        // ① 前シーンフェードアウト
        var fadeOutController = GameObject.FindObjectOfType<FadeController>();
        if (fadeOutController != null)
            yield return fadeOutController.FadeOut();

        // ② Loadingシーン非同期ロード（遷移待機）
        AsyncOperation op = SceneManager.LoadSceneAsync(nextSceneName);
        op.allowSceneActivation = false;

        // ③ Loading演出再生
        var controller = GameObject.FindObjectOfType<LoadingSceneController>();
        if (controller != null)
            yield return controller.PlayLoadingAnimation();

        // ④ ロード完了待機
        while (op.progress < 0.9f) yield return null;

        // ⑤ 次シーンに切替
        op.allowSceneActivation = true;
        yield return new WaitUntil(() => op.isDone);

        // ⑥ 次のステートへ
        stateMachine.ChangeState(nextState);
    }
}
