using UnityEngine;

public class TurretController : MonoBehaviour
{
    private TurretStateMachine stateMachine;
    
    public TurretPatrolState PatrolState { get; private set; }
    
    [Header("Assembly Component References")]
    [SerializeField] private Transform sliderPivot;        // Moves local Y
    [SerializeField] private Transform rotatorPivot;       // Tilts local X
    [SerializeField] private Transform shooterPivot;       // Spins local Y

    [Header("Patrol Settings - Movement")]
    [SerializeField] private float slideStepDistance = 0.5f;
    [SerializeField] private float slideSpeed = 1f;

    [Header("Patrol Settings - Rotations")]
    [SerializeField] private float targetPitchUpAngle = -25f;
    [SerializeField] private float targetPitchDownAngle = 10f;
    [SerializeField] private float targetYawRightAngle = 45f;
    [SerializeField] private float rotationSpeed = 45f; // Degrees per second

    // Public properties so our Patrol State script can read these values safely
    public Transform SliderPivot => sliderPivot;
    public Transform RotatorPivot => rotatorPivot;
    public Transform ShooterPivot => shooterPivot;
    public float SlideStepDistance => slideStepDistance;
    public float SlideSpeed => slideSpeed;
    public float TargetPitchUpAngle => targetPitchUpAngle;
    public float TargetPitchDownAngle => targetPitchDownAngle;
    public float TargetYawRightAngle => targetYawRightAngle;
    public float RotationSpeed => rotationSpeed;
    private void Awake()
    {
        stateMachine = new TurretStateMachine();

        PatrolState = new TurretPatrolState(this);
    }

    private void Start()
    {
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
        // Check if the thing we bumped into has our TurretBoundary script attached
        if (other.TryGetComponent<TurretBoundary>(out TurretBoundary boundary))
        {
            // Update our patrol state's direction instantly!
            PatrolState.SetVerticalDirection(boundary.DirectionToSet);
        }
    }
}