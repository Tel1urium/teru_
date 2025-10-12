using NUnit.Framework.Interfaces;
using Unity.Loading;
using UnityEngine;

public class StageState : GameState
{
    private int stageNum;

    public StageState(GameStateMachine sm, int num) : base(sm)
    {
        stageNum = num;
    }

    public override string GetSceneName() => $"Stage{stageNum}Scene";

    public override void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) // ƒNƒŠƒA/I—¹‰¼ğŒ
        {
            stateMachine.ChangeState(
                new LoadingState(stateMachine, "ResultScene", new ResultState(stateMachine))
            );
        }
    }
}
