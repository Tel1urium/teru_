using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    private GameStateMachine stateMachine;

    void Start()
    {
        stateMachine = gameObject.AddComponent<GameStateMachine>();
        stateMachine.ChangeState(new TitleState(stateMachine));
    }
}
