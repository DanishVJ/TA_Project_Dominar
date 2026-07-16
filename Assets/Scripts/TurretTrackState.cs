using UnityEngine;

public class TurretTrackState : IState
{
    private TurretController _controller;

    // How close the turret's barrel must be to pointing directly at the player (in degrees)
    private float _aimAccuracyThreshold = 3.0f; 
    // How close the height slider must be to the player's height (in units)
    private float _heightAccuracyThreshold = 0.1f;

    public TurretTrackState(TurretController controller)
    {
        _controller = controller;
    }

    public void Enter()
    {
        Debug.Log("[STATE] Entered TRACK STATE. Aligning physical weapon systems...");
    }

    public void Execute()
    {
        Transform target = _controller.Sensor.DetectedTarget;
        
        // --- PLAYER LOST: Go straight back to patrol ---
        if (target == null)
        {
            _controller.SwitchState(_controller.PatrolState);
            return;
        }
        
        // --- PLAYER FOUND: Track rotations AND slider height ---
        TrackTargetWithPivots(target.position);
        TrackTargetHeightWithSlider(target.position);
        
        // --- PRECISION ALIGNMENT CHECK ---
        if (IsAlignedWithTarget(target.position))
        {
            Debug.Log("[STATE] Perfect target lock acquired! Switching to Attack.");
            _controller.SwitchState(_controller.AttackState);
        }
    }

    public void Exit()
    {
        Debug.Log("[STATE] Exited TRACK STATE.");
    }

    private bool IsAlignedWithTarget(Vector3 targetPosition)
    {
        // 1. Check if the slider has finished matching the player's height
        Vector3 localTargetPos = _controller.SliderPivot.parent.InverseTransformPoint(targetPosition);
        float heightDifference = Mathf.Abs(_controller.SliderPivot.localPosition.y - localTargetPos.y);
        bool heightAligned = heightDifference < _heightAccuracyThreshold;

        // 2. Check if the physical barrel (FirePoint) is looking closely at the player
        Vector3 directionToPlayer = (targetPosition - _controller.FirePoint.position).normalized; 
        float angleToPlayer = Vector3.Angle(_controller.FirePoint.forward, directionToPlayer);     
        bool rotationAligned = angleToPlayer < _aimAccuracyThreshold;

        return heightAligned && rotationAligned;
    }

    private void TrackTargetHeightWithSlider(Vector3 targetPosition)
    {
        Vector3 localTargetPos = _controller.SliderPivot.parent.InverseTransformPoint(targetPosition);
        float targetLocalY = localTargetPos.y;
        
        Vector3 targetSliderPos = new Vector3(_controller.SliderPivot.localPosition.x, targetLocalY, _controller.SliderPivot.localPosition.z);
        
        _controller.SliderPivot.localPosition = Vector3.MoveTowards(
            _controller.SliderPivot.localPosition,
            targetSliderPos,
            _controller.TrackSlideSpeed * Time.deltaTime 
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