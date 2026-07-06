using UnityEngine;

public class TurretPatrolState : IState
{
    private TurretController _turret;
    
    private enum PatrolPhase { MovingUp, YawingRight, PitchingDown, YawingLeft }
    private PatrolPhase _currentPhase;
    
    private Vector3 _targetPosition;
    private Quaternion _targetRotationPitch;
    private Quaternion _targetRotationYaw;
    
    private Quaternion _initialShooterRotation;
    private int _verticalDirection = 1;
    
    public TurretPatrolState(TurretController controller)
    {
        this._turret = controller;
    }

    public void Enter()
    {
        Debug.Log("Turret entered PATROL state.");
        
        _initialShooterRotation = _turret.ShooterPivot.localRotation;
        
        StartPhase(PatrolPhase.MovingUp);
    }

    public void Execute()
    {
        switch (_currentPhase)
        {
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
    }
    
    private void StartPhase(PatrolPhase newPhase)
    {
        _currentPhase = newPhase;

        switch (_currentPhase)
        {
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