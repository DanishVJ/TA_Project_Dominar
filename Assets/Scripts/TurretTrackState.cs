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
        
        // --- PLAYER LOST: Smoothly return to center first ---
        if (target == null)
        {
            _controller.ShooterPivot.localRotation = Quaternion.RotateTowards(
                _controller.ShooterPivot.localRotation,
                _controller.AbsoluteBaseShooterRotation,
                _controller.RotationSpeed * Time.deltaTime
            );

            _controller.RotatorPivot.localRotation = Quaternion.RotateTowards(
                _controller.RotatorPivot.localRotation,
                _controller.AbsoluteBaseRotatorRotation,
                _controller.RotationSpeed * Time.deltaTime
            );

            bool yawCentered = Quaternion.Angle(_controller.ShooterPivot.localRotation, _controller.AbsoluteBaseShooterRotation) < 0.5f;
            bool pitchCentered = Quaternion.Angle(_controller.RotatorPivot.localRotation, _controller.AbsoluteBaseRotatorRotation) < 0.5f;

            if (yawCentered && pitchCentered)
            {
                _controller.SwitchState(_controller.PatrolState);
            }
            return;
        }
        
        // --- PLAYER FOUND: Track rotations AND slider height ---
        TrackTargetWithPivots(target.position);
        TrackTargetHeightWithSlider(target.position);
        
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

    // --- NEW: Continuously slides up/down to match player's height ---
    private void TrackTargetHeightWithSlider(Vector3 targetPosition)
    {
        // Get the target's position relative to the slider's parent structure
        Vector3 localTargetPos = _controller.SliderPivot.parent.InverseTransformPoint(targetPosition);
        
        // Isolate the height target (Y axis)
        float targetLocalY = localTargetPos.y;
        
        // Target position keeping our current local X and Z intact
        Vector3 targetSliderPos = new Vector3(_controller.SliderPivot.localPosition.x, targetLocalY, _controller.SliderPivot.localPosition.z);
        
        // Smoothly slide toward the target player height using SlideSpeed
        _controller.SliderPivot.localPosition = Vector3.MoveTowards(
            _controller.SliderPivot.localPosition,
            targetSliderPos,
            _controller.SlideSpeed * Time.deltaTime
        );
    }

    private void TrackTargetWithPivots(Vector3 targetPosition)
    {
        // --- YAW (Left / Right) ---
        Vector3 localTargetPosForYaw = _controller.ShooterPivot.parent.InverseTransformPoint(targetPosition);
        localTargetPosForYaw.y = _controller.ShooterPivot.localPosition.y;

        Vector3 localDirYaw = localTargetPosForYaw - _controller.ShooterPivot.localPosition;
        if (localDirYaw != Vector3.zero)
        {
            Quaternion targetLocalYaw = Quaternion.LookRotation(localDirYaw, Vector3.up);
            float rawYawAngle = targetLocalYaw.eulerAngles.y;
            float relativeYaw = Mathf.DeltaAngle(_controller.AbsoluteBaseShooterRotation.eulerAngles.y, rawYawAngle);
            float clampedRelativeYaw = Mathf.Clamp(relativeYaw, -_controller.MaxTrackingYaw, _controller.MaxTrackingYaw);
            float finalYawAngle = _controller.AbsoluteBaseShooterRotation.eulerAngles.y + clampedRelativeYaw;

            _controller.ShooterPivot.localRotation = Quaternion.RotateTowards(
                _controller.ShooterPivot.localRotation,
                Quaternion.Euler(0, finalYawAngle, 0),
                _controller.RotationSpeed * Time.deltaTime
            );
        }
        
        // --- PITCH (Up / Down) ---
        Vector3 localTargetPosForPitch = _controller.RotatorPivot.parent.InverseTransformPoint(targetPosition);
        Vector3 localDirPitch = localTargetPosForPitch - _controller.RotatorPivot.localPosition;

        if (localDirPitch != Vector3.zero)
        {
            Quaternion targetLocalPitch = Quaternion.LookRotation(localDirPitch, Vector3.up);
            float rawPitchAngle = targetLocalPitch.eulerAngles.x;
            float relativePitch = Mathf.DeltaAngle(_controller.AbsoluteBaseRotatorRotation.eulerAngles.x, rawPitchAngle);
            float clampedRelativePitch = Mathf.Clamp(relativePitch, -_controller.MaxTrackingPitchUp, _controller.MaxTrackingPitchDown);
            float finalPitchAngle = _controller.AbsoluteBaseRotatorRotation.eulerAngles.x + clampedRelativePitch;

            _controller.RotatorPivot.localRotation = Quaternion.RotateTowards(
                _controller.RotatorPivot.localRotation,
                Quaternion.Euler(finalPitchAngle, 0, 0),
                _controller.RotationSpeed * Time.deltaTime
            );
        }
    }
}