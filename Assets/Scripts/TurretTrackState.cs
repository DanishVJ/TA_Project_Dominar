using UnityEngine;

public class TurretTrackState : IState
{
    private TurretController _controller;
    private float _trackTimer;
    private float _timeToLockOn = 1.0f;

    public TurretTrackState(TurretController controller)
    {
        _controller = controller;
    }

    public void Enter()
    {
        _trackTimer = 0f;
        Debug.Log("[STATE] Entered TRACK STATE. Locking onto target...");
    }

    public void Execute()
    {
        Transform target = _controller.Sensor.DetectedTarget;
        
        if (target == null)
        {
            _controller.SwitchState(_controller.PatrolState);
            return;
        }
        
        TrackTargetWithPivots(target.position);
        
        _trackTimer += Time.deltaTime;
        if (_trackTimer >= _timeToLockOn)
        {
            Debug.Log("[STATE] Lock-on complete! Ready to attack!");
        }
    }

    public void Exit()
    {
        Debug.Log("[STATE] Exited TRACK STATE.");
    }

    private void TrackTargetWithPivots(Vector3 targetPosition)
    {

        Vector3 localTargetPosForYaw = _controller.ShooterPivot.parent.InverseTransformPoint(targetPosition);
        localTargetPosForYaw.y = _controller.ShooterPivot.localPosition.y;

        Vector3 localDirYaw = localTargetPosForYaw - _controller.ShooterPivot.localPosition;
        if (localDirYaw != Vector3.zero)
        {
            Quaternion targetLocalYaw = Quaternion.LookRotation(localDirYaw, Vector3.up);
            
            float targetYawAngle = targetLocalYaw.eulerAngles.y;

            _controller.ShooterPivot.localRotation = Quaternion.RotateTowards(
                _controller.ShooterPivot.localRotation,
                Quaternion.Euler(0, targetYawAngle, 0),
                _controller.RotationSpeed * Time.deltaTime
            );
        }
        
        Vector3 localTargetPosForPitch = _controller.RotatorPivot.parent.InverseTransformPoint(targetPosition);
        Vector3 localDirPitch = localTargetPosForPitch - _controller.RotatorPivot.localPosition;

        if (localDirPitch != Vector3.zero)
        {
            Quaternion targetLocalPitch = Quaternion.LookRotation(localDirPitch, Vector3.up);
            
            float targetPitchAngle = targetLocalPitch.eulerAngles.x;

            _controller.RotatorPivot.localRotation = Quaternion.RotateTowards(
                _controller.RotatorPivot.localRotation,
                Quaternion.Euler(targetPitchAngle, 0, 0),
                _controller.RotationSpeed * Time.deltaTime
            );
        }
    }
}