using UnityEngine;

public class TurretAttackState : IState
{
    private TurretController _controller;
    private float _fireTimer;

    public TurretAttackState(TurretController controller)
    {
        _controller = controller;
    }

    public void Enter()
    {
        Debug.Log("[STATE] Entered ATTACK STATE! Commencing firing sequence.");
        // Instantly ready to shoot on enter, or set to _controller.FireRate for an initial delay
        _fireTimer = 0f; 
    }

    public void Execute()
    {
        Transform target = _controller.Sensor.DetectedTarget;

        // --- TARGET LOST ---
        // If player is dead, out of range, or behind cover, fall back to TrackState to handle re-centering
        if (target == null)
        {
            _controller.SwitchState(_controller.TrackState);
            return;
        }

        // --- SUSTAINED TRACKING ---
        // Reuses the controller's target coordinates to keep the gun on target while firing
        TrackTargetWithPivots(target.position);
        TrackTargetHeightWithSlider(target.position);

        // --- FIRING COOLDOWN TIMER ---
        _fireTimer -= Time.deltaTime;
        if (_fireTimer <= 0f)
        {
            _controller.Shoot();
            _fireTimer = _controller.FireRate; // Reset timer
        }
    }

    public void Exit()
    {
        Debug.Log("[STATE] Exited ATTACK STATE.");
    }

    private void TrackTargetHeightWithSlider(Vector3 targetPosition)
    {
        Vector3 localTargetPos = _controller.SliderPivot.parent.InverseTransformPoint(targetPosition);
        float targetLocalY = localTargetPos.y;
        
        Vector3 targetSliderPos = new Vector3(_controller.SliderPivot.localPosition.x, targetLocalY, _controller.SliderPivot.localPosition.z);
        
        _controller.SliderPivot.localPosition = Vector3.MoveTowards(
            _controller.SliderPivot.localPosition,
            targetSliderPos,
            _controller.SlideSpeed * Time.deltaTime
        );
    }

    private void TrackTargetWithPivots(Vector3 targetPosition)
    {
        // --- YAW ---
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
        
        // --- PITCH ---
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