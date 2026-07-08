using UnityEngine;

public class PlayerIdleState : IState
{
    private readonly PlayerController _player;
    private const float MoveThreshold = 0.1f;

    public PlayerIdleState(PlayerController player)
    {
        _player = player;
    }

    public void Enter() { }

    public void Execute()
    {
        // Transition to move
        if (_player.MoveInput.sqrMagnitude > MoveThreshold * MoveThreshold)
        {
            _player.StateMachine.ChangeState(_player.MoveState);
            return;
        }

        // If we start falling, switch to fall
        if (!_player.IsGrounded())
        {
            _player.StateMachine.ChangeState(_player.FallState);
            return;
        }

        // Ensure horizontal velocity is zero while idle
        Vector3 v = _player.Velocity;
        v.x = 0;
        v.z = 0;
        _player.Velocity = v;
    }

    public void Exit() { }
}

