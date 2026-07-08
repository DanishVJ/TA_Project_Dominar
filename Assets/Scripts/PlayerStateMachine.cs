using UnityEngine;

public class PlayerStateMachine
{
    public IState CurrentState { get; private set; }

    public void Initialize(IState initialState)
    {
        CurrentState = initialState;
        CurrentState.Enter();
    }

    public void ChangeState(IState newState)
    {
        if (CurrentState == newState) return;

        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }

    public void ExecuteActiveState()
    {
        CurrentState?.Execute();
    }
}

