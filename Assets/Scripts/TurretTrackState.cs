using UnityEngine;

public class TurretTrackState : IState
{
    private TurretController _controller;
    private float _aimAccuracyThreshold = 3.0f; // Degrees away from perfect lock-on to swap to attack

    public TurretTrackState(TurretController controller)
    {
        _controller = controller;
    }

    public void Enter()
    {
        Debug.Log("[STATE] Entered TRACK state.");
    }

    public void Execute()
    {
        Transform target = _controller.Sensor.DetectedTarget;

        if (target == null)
        {
            // Frame-accurate loss handling: drops back to patrol instantly if no target
            _controller.SwitchState(_controller.PatrolState);
            return;
        }

        // FIX: Grab center mass vector instead of feet position
        Vector3 targetAimPoint = _controller.Sensor.TargetCenterPosition;

        TrackTargetWithPivots(targetAimPoint);
        TrackTargetHeightWithSlider(targetAimPoint);

        // Check weapon barrel alignment angles against center mass
        Vector3 dirToTarget = (targetAimPoint - _controller.FirePoint.position).normalized;
        float angleToTarget = Vector3.Angle(_controller.FirePoint.forward, dirToTarget);

        if (angleToTarget <= _aimAccuracyThreshold)
        {
            Debug.Log("[TRACK] Target aligned! Switching to ATTACK state.");
            _controller.SwitchState(_controller.AttackState);
        }
    }

    public void Exit()
    {
        Debug.Log("[STATE] Exited TRACK state.");
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