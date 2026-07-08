using UnityEngine;

public class PlayerJumpState : IState
{
    private readonly PlayerController _player;

    public PlayerJumpState(PlayerController player)
    {
        _player = player;
    }

    public void Enter()
    {
        Debug.Log($"PlayerJumpState.Enter: applying jump velocity={_player.JumpVelocity}");
        // Apply jump velocity
        Vector3 v = _player.Velocity;
        v.y = _player.JumpVelocity;
        _player.Velocity = v;

        // Use controller helper to raise event (events cannot be invoked from outside declaring type)
        _player.RaiseJumpEvent();
    }

    public void Execute()
    {
        // Apply gravity
        Vector3 camForward = _player.PlayerCamera.transform.forward;
        Vector3 camRight = _player.PlayerCamera.transform.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDirection = camRight * _player.MoveInput.x + camForward * _player.MoveInput.y;
        Vector3 horizontal = _player.MoveSpeed * moveDirection;

        float y = _player.Velocity.y;
        y += _player.Gravity * Time.deltaTime;

        _player.Velocity = horizontal + Vector3.up * y;
        _player.CharacterController.Move(_player.Velocity * Time.deltaTime);

        // When we stop rising, go to fall
        if (_player.Velocity.y <= 0f)
        {
            _player.StateMachine.ChangeState(_player.FallState);
        }
    }

    public void Exit() { }
}

