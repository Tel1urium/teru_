using UnityEngine;

public abstract class GameState
{
    protected GameStateMachine stateMachine;

    public GameState(GameStateMachine sm)
    {
        stateMachine = sm;
    }

    public abstract string GetSceneName();
    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void Exit() { }
}