using UnityEngine;

public class TurretAttackState : IState
{
    private TurretController _controller;
    private float _fireCooldownTimer = 0f;
    private float _lostTargetTimer = 0f;
    private const float TARGET_LOSS_GRACE_DURATION = 1.0f; // Grace window in seconds

    public TurretAttackState(TurretController controller)
    {
        _controller = controller;
    }

    public void Enter()
    {
        Debug.Log("[STATE] Entered ATTACK state.");
        _lostTargetTimer = 0f;
        _fireCooldownTimer = 0f; // Ready to shoot instantly upon entering range
    }

    public void Execute()
    {
        Transform target = _controller.Sensor.DetectedTarget;

        if (target == null)
        {
            // Activate target-loss countdown grace period instead of instantly dropping state
            _lostTargetTimer += Time.deltaTime;
            if (_lostTargetTimer >= TARGET_LOSS_GRACE_DURATION)
            {
                Debug.Log("[ATTACK] Target lost permanently. Retreating to Patrol.");
                _controller.SwitchState(_controller.PatrolState);
            }
            return;
        }

        // Reset target loss window if tracking holds true
        _lostTargetTimer = 0f;

        // FIX: Grab center mass vector instead of feet position
        Vector3 targetAimPoint = _controller.Sensor.TargetCenterPosition;

        TrackTargetWithPivots(targetAimPoint);
        TrackTargetHeightWithSlider(targetAimPoint);

        // Run fire rate handling checks
        _fireCooldownTimer -= Time.deltaTime;
        if (_fireCooldownTimer <= 0f)
        {
            _controller.Shoot();
            _fireCooldownTimer = _controller.FireRate;
        }
    }

    public void Exit()
    {
        Debug.Log("[STATE] Exited ATTACK state.");
    }

    private void TrackTargetWithPivots(Vector3 targetPosition)
{
    // --- 1. ROTATOR PIVOT (PITCH ONLY - LOCAL X ROTATION) ---
    // Get target position relative to the Rotator's direct parent (SliderPivot)
    Vector3 targetInRotatorParentSpace = _controller.RotatorPivot.parent.InverseTransformPoint(targetPosition);
    Vector3 dirToTargetPitch = targetInRotatorParentSpace - _controller.RotatorPivot.localPosition;

    if (dirToTargetPitch.z != 0f || dirToTargetPitch.y != 0f)
    {
        // Calculate the clean local angle target using Atan2
        float targetPitchAngle = Mathf.Atan2(dirToTargetPitch.y, dirToTargetPitch.z) * Mathf.Rad2Deg;
        float relativePitch = -targetPitchAngle; // Invert because Unity positive X rotates downward
        
        float clampedRelativePitch = Mathf.Clamp(relativePitch, -_controller.MaxTrackingPitchUp, _controller.MaxTrackingPitchDown);
        
        // FIX: Directly rotate around local X axis relative to identity, NO static base multiplier overrides
        Quaternion targetLocalPitch = Quaternion.Euler(clampedRelativePitch, 0f, 0f);

        _controller.RotatorPivot.localRotation = Quaternion.RotateTowards(
            _controller.RotatorPivot.localRotation,
            targetLocalPitch,
            _controller.RotationSpeed * Time.deltaTime
        );
    }

    // --- 2. SHOOTER PIVOT (YAW ONLY - LOCAL Y ROTATION) ---
    // Get target position relative to the Shooter's direct parent (RotatorPivot)
    Vector3 targetInShooterParentSpace = _controller.ShooterPivot.parent.InverseTransformPoint(targetPosition);
    Vector3 dirToTargetYaw = targetInShooterParentSpace - _controller.ShooterPivot.localPosition;

    if (dirToTargetYaw.x != 0f || dirToTargetYaw.z != 0f)
    {
        // Calculate clean local horizontal angle using Atan2
        float targetYawAngle = Mathf.Atan2(dirToTargetYaw.x, dirToTargetYaw.z) * Mathf.Rad2Deg;
        
        float clampedRelativeYaw = Mathf.Clamp(targetYawAngle, -_controller.MaxTrackingYaw, _controller.MaxTrackingYaw);
        
        // FIX: Directly rotate around local Y axis relative to identity
        Quaternion targetLocalYaw = Quaternion.Euler(0f, clampedRelativeYaw, 0f);

        _controller.ShooterPivot.localRotation = Quaternion.RotateTowards(
            _controller.ShooterPivot.localRotation,
            targetLocalYaw,
            _controller.RotationSpeed * Time.deltaTime
        );
    }
}

    private void TrackTargetHeightWithSlider(Vector3 targetPosition)
    {
        float targetY = targetPosition.y;
        Vector3 currentLocalPos = _controller.SliderPivot.localPosition;
        Vector3 targetLocalPos = _controller.SliderPivot.parent.InverseTransformPoint(new Vector3(targetPosition.x, targetY, targetPosition.z));
        
        float newLocalY = Mathf.MoveTowards(currentLocalPos.y, targetLocalPos.y, _controller.TrackSlideSpeed * Time.deltaTime);
        _controller.SliderPivot.localPosition = new Vector3(currentLocalPos.x, newLocalY, currentLocalPos.z);
    }
}