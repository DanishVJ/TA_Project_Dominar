using UnityEngine;

public class TurretController : MonoBehaviour
{
    private TurretStateMachine stateMachine;
    
    public TurretPatrolState PatrolState { get; private set; }
    public TurretTrackState TrackState { get; private set; }
    
    [Header("Assembly Component References")]
    [SerializeField] private Transform sliderPivot;
    [SerializeField] private Transform rotatorPivot;
    [SerializeField] private Transform shooterPivot;
    [SerializeField] private TurretSensor turretSensor;

    [Header("Patrol Settings - Movement")]
    [SerializeField] private float slideStepDistance = 0.5f;
    [SerializeField] private float slideSpeed = 1f;

    [Header("Patrol Settings - Rotations")]
    [SerializeField] private float targetPitchUpAngle = -25f;
    [SerializeField] private float targetPitchDownAngle = 10f;
    [SerializeField] private float targetYawRightAngle = 45f;
    [SerializeField] private float rotationSpeed = 45f;

    [Header("Track Constraints")] [SerializeField]
    private float maxTrackingYaw = 60f;
    [SerializeField] private float maxTrackingPitchUp = 30f;
    [SerializeField] private float maxTrackingPitchDown = 15f;
    
    public Transform SliderPivot => sliderPivot;
    public Transform RotatorPivot => rotatorPivot;
    public Transform ShooterPivot => shooterPivot;
    public TurretSensor Sensor => turretSensor;
    public float SlideStepDistance => slideStepDistance;
    public float SlideSpeed => slideSpeed;
    public float TargetPitchUpAngle => targetPitchUpAngle;
    public float TargetPitchDownAngle => targetPitchDownAngle;
    public float TargetYawRightAngle => targetYawRightAngle;
    public float RotationSpeed => rotationSpeed;
    public float MaxTrackingYaw => maxTrackingYaw;
    public float MaxTrackingPitchUp => maxTrackingPitchUp;
    public float MaxTrackingPitchDown => maxTrackingPitchDown;
    
    public Quaternion AbsoluteBaseShooterRotation { get; private set; }
    public Quaternion AbsoluteBaseRotatorRotation { get; private set; }
    
    private void Awake()
    {
        stateMachine = new TurretStateMachine();

        PatrolState = new TurretPatrolState(this);
        TrackState = new TurretTrackState(this);
    }

    private void Start()
    {
        AbsoluteBaseShooterRotation = shooterPivot.localRotation;
        AbsoluteBaseRotatorRotation = rotatorPivot.localRotation;
        
        stateMachine.Initialize(PatrolState);
    }

    private void Update()
    {
        stateMachine.ExecuteActiveState();
    }

    public void SwitchState(IState newState)
    {
        stateMachine.ChangeState(newState);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Turret collided with: {other.gameObject.name}");
        if (other.TryGetComponent<TurretBoundary>(out TurretBoundary boundary))
        {
            PatrolState.SetVerticalDirection(boundary.DirectionToSet);
        }
    }
}