using UnityEngine;

public class GameStateMachine : MonoBehaviour
{
    private GameState currentState;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void ChangeState(GameState newState)
    {
        if (currentState != null)
            currentState.Exit();

        currentState = newState;

        if (currentState != null)
            currentState.Enter();
    }

    private void Update()
    {
        if (currentState != null)
            currentState.Update();
    }
}
