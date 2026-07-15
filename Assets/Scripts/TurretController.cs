using UnityEngine;

public class TurretController : MonoBehaviour
{
    private TurretStateMachine stateMachine;
    
    public TurretPatrolState PatrolState { get; private set; }
    public TurretTrackState TrackState { get; private set; }
    public TurretAttackState AttackState { get; private set; } // Added Attack State
    
    [Header("Assembly Component References")]
    [SerializeField] private Transform sliderPivot;
    [SerializeField] private Transform rotatorPivot;
    [SerializeField] private Transform shooterPivot;
    [SerializeField] private TurretSensor turretSensor;

    [Header("Weapon Settings")]
    [SerializeField] private GameObject projectilePrefab;       // Assign a simple bullet prefab here
    [SerializeField] private Transform firePoint;                // Assign where the bullet spawns from
    [SerializeField] private float fireRate = 1.0f;              // Seconds between shots
    [SerializeField] private float projectileSpeed = 20f;        // Bullet speed

    [Header("Strategy Pattern (Scriptable Objects)")]
    [SerializeField] private ProjectileAttackStrategy projectileStrategy;
    [SerializeField] private LaserAttackStrategy laserStrategy;
    [Tooltip("If the player is closer than this distance, use Projectiles. If further, use Lasers.")]
    [SerializeField] private float strategySwitchDistance = 8f;

    [Header("Patrol Settings - Movement")]
    [SerializeField] private float slideStepDistance = 0.5f;
    [SerializeField] private float slideSpeed = 1f;

    [Header("Track Settings - Speed Adjustments")]
    [Tooltip("How fast the turret slides up and down when tracking the player.")]
    [SerializeField] private float trackSlideSpeed = 4f; // Snappy speed for active tracking!

    [Header("Patrol Settings - Rotations")]
    [SerializeField] private float targetPitchUpAngle = -25f;
    [SerializeField] private float targetPitchDownAngle = 10f;
    [SerializeField] private float targetYawRightAngle = 45f;
    [SerializeField] private float rotationSpeed = 45f;

    [Header("Track Constraints")] 
    [SerializeField] private float maxTrackingYaw = 60f;
    [SerializeField] private float maxTrackingPitchUp = 30f;
    [SerializeField] private float maxTrackingPitchDown = 15f;
    
    // Strategy runtime variable
    private IAttackStrategy _activeStrategy;

    public Transform SliderPivot => sliderPivot;
    public Transform RotatorPivot => rotatorPivot;
    public Transform ShooterPivot => shooterPivot;
    public TurretSensor Sensor => turretSensor;
    public float SlideStepDistance => slideStepDistance;
    public float SlideSpeed => slideSpeed;
    public float TrackSlideSpeed => trackSlideSpeed; // Getter for TrackState to read
    public float TargetPitchUpAngle => targetPitchUpAngle;
    public float TargetPitchDownAngle => targetPitchDownAngle;
    public float TargetYawRightAngle => targetYawRightAngle;
    public float RotationSpeed => rotationSpeed;
    public float MaxTrackingYaw => maxTrackingYaw;
    public float MaxTrackingPitchUp => maxTrackingPitchUp;
    public float MaxTrackingPitchDown => maxTrackingPitchDown;

    // Getters for Weapon Settings
    public float FireRate => fireRate;
    public Transform FirePoint => firePoint; // <-- CapitalIZED 'F' to prevent name collisions!
    
    public Quaternion AbsoluteBaseShooterRotation { get; private set; }
    public Quaternion AbsoluteBaseRotatorRotation { get; private set; }
    
    private void Awake()
    {
        stateMachine = new TurretStateMachine();

        PatrolState = new TurretPatrolState(this);
        TrackState = new TurretTrackState(this);
        AttackState = new TurretAttackState(this); // Initialized Attack State
    }

    private void Start()
    {
        AbsoluteBaseShooterRotation = shooterPivot.localRotation;
        AbsoluteBaseRotatorRotation = rotatorPivot.localRotation;
        
        // Default strategy on startup
        _activeStrategy = projectileStrategy;

        stateMachine.Initialize(PatrolState);
    }

    private void Update()
    {
        stateMachine.ExecuteActiveState();

        // Dynamically choose the strategy based on player distance
        EvaluateStrategy();
    }

    private void EvaluateStrategy()
    {
        if (Sensor.DetectedTarget == null) return;

        float distanceToTarget = Vector3.Distance(transform.position, Sensor.DetectedTarget.position);

        if (distanceToTarget < strategySwitchDistance)
        {
            if (_activeStrategy != (IAttackStrategy)projectileStrategy)
            {
                _activeStrategy = projectileStrategy;
                Debug.Log("[STRATEGY] Player is CLOSE. Swapping to PROJECTILE strategy.");
            }
        }
        else
        {
            if (_activeStrategy != (IAttackStrategy)laserStrategy)
            {
                _activeStrategy = laserStrategy;
                Debug.Log("[STRATEGY] Player is FAR. Swapping to LASER strategy.");
            }
        }
    }

    public void SwitchState(IState newState)
    {
        stateMachine.ChangeState(newState);
    }

    // Instantiates the projectile and shoots it forward using active Strategy
    public void Shoot()
    {
        if (_activeStrategy != null)
        {
            _activeStrategy.Attack(firePoint, projectilePrefab, projectileSpeed, 0f);
        }
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