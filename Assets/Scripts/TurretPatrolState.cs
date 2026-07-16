using UnityEngine;

public class TurretPatrolState : IState
{
    private TurretController _turret;
    
    // Added a "ReturningToStart" phase to handle the transition smoothly
    private enum PatrolPhase { ReturningToStart, MovingUp, YawingRight, PitchingDown, YawingLeft }
    private PatrolPhase _currentPhase;
    
    private Vector3 _targetPosition;
    private Quaternion _targetRotationPitch;
    private Quaternion _targetRotationYaw;
    
    private Quaternion _initialShooterRotation;
    private int _verticalDirection = 1;
    
    public TurretPatrolState(TurretController controller)
    {
        _turret = controller;
    }

    public void Enter()
    {
        Debug.Log("Turret entered PATROL state.");
        
        _initialShooterRotation = _turret.AbsoluteBaseShooterRotation;

        float currentPitch = Mathf.DeltaAngle(0, _turret.RotatorPivot.localEulerAngles.x);
    
        if (currentPitch < -5f) 
        {
            _verticalDirection = 1; // Go up
        }
        else if (currentPitch > 5f)
        {
            _verticalDirection = -1; // Go down
        }
        
        // Start by smoothly returning to the default starting rotations
        StartPhase(PatrolPhase.ReturningToStart);
    }

    public void Execute()
    {
        if (_turret.Sensor.DetectedTarget != null)
        {
            _turret.SwitchState(_turret.TrackState);
            return;
        }
        
        switch (_currentPhase)
        {
            case PatrolPhase.ReturningToStart:
                HandleReturningToStart();
                break;
            case PatrolPhase.MovingUp:
                HandleMovingUp();
                break;
            case PatrolPhase.YawingRight:
                HandleYawingRight();
                break;
            case PatrolPhase.PitchingDown:
                HandlePitchingDown();
                break;
            case PatrolPhase.YawingLeft:
                HandleYawingLeft();
                break;
        }
    }

    public void Exit()
    {
        Debug.Log("Turret exited PATROL state.");
    }
    
    public void SetVerticalDirection(int newDirection)
    {
        _verticalDirection = newDirection;
        
        if (_currentPhase == PatrolPhase.MovingUp)
        {
            _targetPosition = _turret.SliderPivot.localPosition + new Vector3(0, _turret.SlideStepDistance * _verticalDirection, 0);
        }
    }
    
    private void StartPhase(PatrolPhase newPhase)
    {
        _currentPhase = newPhase;

        switch (_currentPhase)
        {
            case PatrolPhase.ReturningToStart:
                // Set targets to default starting rotations
                _targetRotationYaw = _initialShooterRotation;
                _targetRotationPitch = Quaternion.Euler(_turret.TargetPitchUpAngle, 0, 0);
                break;

            case PatrolPhase.MovingUp:
                _targetPosition = _turret.SliderPivot.localPosition + new Vector3(0, _turret.SlideStepDistance * _verticalDirection, 0);
                _targetRotationPitch = Quaternion.Euler(_turret.TargetPitchUpAngle, 0, 0);
                break;

            case PatrolPhase.YawingRight:
                _targetRotationYaw = Quaternion.Euler(0, _turret.TargetYawRightAngle, 0);
                break;

            case PatrolPhase.PitchingDown:
                _targetRotationPitch = Quaternion.Euler(_turret.TargetPitchDownAngle, 0, 0);
                break;

            case PatrolPhase.YawingLeft:
                _targetRotationYaw = _initialShooterRotation;
                break;
        }
    }

    private void HandleReturningToStart()
    {
        // Smoothly rotate horizontal (Yaw) and vertical (Pitch) back to default patrol starts
        _turret.ShooterPivot.localRotation = Quaternion.RotateTowards(
            _turret.ShooterPivot.localRotation, 
            _targetRotationYaw, 
            _turret.RotationSpeed * Time.deltaTime
        );

        _turret.RotatorPivot.localRotation = Quaternion.RotateTowards(
            _turret.RotatorPivot.localRotation, 
            _targetRotationPitch, 
            _turret.RotationSpeed * Time.deltaTime
        );

        bool yawAligned = Quaternion.Angle(_turret.ShooterPivot.localRotation, _targetRotationYaw) < 0.5f;
        bool pitchAligned = Quaternion.Angle(_turret.RotatorPivot.localRotation, _targetRotationPitch) < 0.5f;

        // Once fully aligned to our start values, safely kick off the patrol sequence!
        if (yawAligned && pitchAligned)
        {
            StartPhase(PatrolPhase.MovingUp);
        }
    }
    
    private void HandleMovingUp()
    {
        _turret.SliderPivot.localPosition = Vector3.MoveTowards(
            _turret.SliderPivot.localPosition, 
            _targetPosition, 
            _turret.SlideSpeed * Time.deltaTime
        );
        
        _turret.RotatorPivot.localRotation = Quaternion.RotateTowards(
            _turret.RotatorPivot.localRotation, 
            _targetRotationPitch, 
            _turret.RotationSpeed * Time.deltaTime
        );
        
        bool positionReached = Vector3.Distance(_turret.SliderPivot.localPosition, _targetPosition) < 0.01f;
        bool rotationReached = Quaternion.Angle(_turret.RotatorPivot.localRotation, _targetRotationPitch) < 0.5f;

        if (positionReached && rotationReached)
        {
            StartPhase(PatrolPhase.YawingRight);
        }
    }

    private void HandleYawingRight()
    {
        _turret.ShooterPivot.localRotation = Quaternion.RotateTowards(
            _turret.ShooterPivot.localRotation, 
            _targetRotationYaw, 
            _turret.RotationSpeed * Time.deltaTime
        );
        
        if (Quaternion.Angle(_turret.ShooterPivot.localRotation, _targetRotationYaw) < 0.5f)
        {
            StartPhase(PatrolPhase.PitchingDown);
        }
    }

    private void HandlePitchingDown()
    {
        _turret.RotatorPivot.localRotation = Quaternion.RotateTowards(
            _turret.RotatorPivot.localRotation, 
            _targetRotationPitch, 
            _turret.RotationSpeed * Time.deltaTime
        );
        
        if (Quaternion.Angle(_turret.RotatorPivot.localRotation, _targetRotationPitch) < 0.5f)
        {
            StartPhase(PatrolPhase.YawingLeft);
        }
    }

    private void HandleYawingLeft()
    {
        _turret.ShooterPivot.localRotation = Quaternion.RotateTowards(
            _turret.ShooterPivot.localRotation, 
            _targetRotationYaw, 
            _turret.RotationSpeed * Time.deltaTime
        );
        
        if (Quaternion.Angle(_turret.ShooterPivot.localRotation, _targetRotationYaw) < 0.5f)
        {
            StartPhase(PatrolPhase.MovingUp);
        }
    }
}