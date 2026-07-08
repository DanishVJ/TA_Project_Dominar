using UnityEngine;

public class PlayerMoveState : IState
{
    private readonly PlayerController _player;
    private const float MoveThreshold = 0.01f;

    public PlayerMoveState(PlayerController player)
    {
        _player = player;
    }

    public void Enter() { }

    public void Execute()
    {
        // If we lost ground -> Fall
        if (!_player.IsGrounded())
        {
            _player.StateMachine.ChangeState(_player.FallState);
            return;
        }

        Vector3 camForward = _player.PlayerCamera.transform.forward;
        Vector3 camRight = _player.PlayerCamera.transform.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDirection = camRight * _player.MoveInput.x + camForward * _player.MoveInput.y;

        if (moveDirection.sqrMagnitude > MoveThreshold)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            _player.transform.rotation = Quaternion.Slerp(_player.transform.rotation, targetRotation, _player.RotationSpeed * Time.deltaTime);
        }

        // Horizontal velocity
        Vector3 horizontal = _player.MoveSpeed * moveDirection;
        float y = _player.Velocity.y;
        y += _player.Gravity * Time.deltaTime;
        _player.Velocity = horizontal + Vector3.up * y;

        _player.CharacterController.Move(_player.Velocity * Time.deltaTime);

        // Transition to idle if no input
        if (_player.MoveInput.sqrMagnitude <= MoveThreshold)
        {
            _player.StateMachine.ChangeState(_player.IdleState);
        }
    }

    public void Exit() { }
}

