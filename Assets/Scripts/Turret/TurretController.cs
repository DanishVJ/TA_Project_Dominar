using UnityEngine;

public class TurretController : MonoBehaviour
{
    private TurretStateMachine stateMachine;
    
    public TurretPatrolState PatrolState { get; private set; }
    public TurretTrackState TrackState { get; private set; }
    public TurretAttackState AttackState { get; private set; }
    
    [Header("Visual Indicators")]
    [SerializeField] private Light turretSpotLight;              // Drag your spotlight here in Unity!

    [Header("Assembly Component References")]
    [SerializeField] private Transform sliderPivot;
    [SerializeField] private Transform rotatorPivot;
    [SerializeField] private Transform shooterPivot;
    [SerializeField] private TurretSensor turretSensor;

    [Header("Weapon Settings")]
    [SerializeField] private GameObject projectilePrefab;       
    [SerializeField] private Transform firePoint;                
    [SerializeField] private float fireRate = 1.0f;              
    [SerializeField] private float projectileSpeed = 20f;        

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
    [SerializeField] private float trackSlideSpeed = 4f; 

    [Header("Patrol Settings - Rotations")]
    [SerializeField] private float targetPitchUpAngle = -25f;
    [SerializeField] private float targetPitchDownAngle = 10f;
    [SerializeField] private float targetYawRightAngle = 45f;
    [SerializeField] private float rotationSpeed = 45f;

    [Header("Track Constraints")] 
    [SerializeField] private float maxTrackingYaw = 60f;
    [SerializeField] private float maxTrackingPitchUp = 30f;
    [SerializeField] private float maxTrackingPitchDown = 15f;
    
    [Header("Shutdown Settings")]
    [SerializeField] private AudioSource turretAudioSource;
    [SerializeField] private AudioClip successClip;
    [SerializeField] private AudioClip shutdownClip;
    
    private bool isDeactivated = false;
    
    private IAttackStrategy _activeStrategy;

    public Transform SliderPivot => sliderPivot;
    public Transform RotatorPivot => rotatorPivot;
    public Transform ShooterPivot => shooterPivot;
    public TurretSensor Sensor => turretSensor;
    public float SlideStepDistance => slideStepDistance;
    public float SlideSpeed => slideSpeed;
    public float TrackSlideSpeed => trackSlideSpeed; 
    public float TargetPitchUpAngle => targetPitchUpAngle;
    public float TargetPitchDownAngle => targetPitchDownAngle;
    public float TargetYawRightAngle => targetYawRightAngle;
    public float RotationSpeed => rotationSpeed;
    public float MaxTrackingYaw => maxTrackingYaw;
    public float MaxTrackingPitchUp => maxTrackingPitchUp;
    public float MaxTrackingPitchDown => maxTrackingPitchDown;

    public float FireRate => fireRate;
    public Transform FirePoint => firePoint; 
    
    public Quaternion AbsoluteBaseShooterRotation { get; private set; }
    public Quaternion AbsoluteBaseRotatorRotation { get; private set; }
    
    private void Awake()
    {
        stateMachine = new TurretStateMachine();

        PatrolState = new TurretPatrolState(this);
        TrackState = new TurretTrackState(this);
        AttackState = new TurretAttackState(this); 
    }

    private void Start()
    {
        AbsoluteBaseShooterRotation = shooterPivot.localRotation;
        AbsoluteBaseRotatorRotation = rotatorPivot.localRotation;
        
        _activeStrategy = projectileStrategy;

        stateMachine.Initialize(PatrolState);
        UpdateLightColor(PatrolState); 
    }

    private void Update()
    {
        if (isDeactivated) return; 

        stateMachine.ExecuteActiveState();
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
        UpdateLightColor(newState); 
    }

    private void UpdateLightColor(IState state)
    {
        TurretLightController lightController = GetComponentInChildren<TurretLightController>();
        if (lightController == null) return;

        if (state == PatrolState)
        {
            lightController.SetPatrolColor();
        }
        else if (state == TrackState)
        {
            lightController.SetTrackColor();
        }
        else if (state == AttackState)
        {
            lightController.SetAttackColor();
        }
    }

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
    
    public void DisableTurret()
    {
        if (isDeactivated) return;
        isDeactivated = true;
        
        if (TurretManager.Instance != null) TurretManager.Instance.TurretDeactivated();

        if (turretAudioSource != null)
        {
            if (successClip != null)
            {
                turretAudioSource.PlayOneShot(successClip);
            }
            
            if (shutdownClip != null)
            {
                turretAudioSource.clip = shutdownClip;
                turretAudioSource.PlayDelayed(1.0f);
            }
        }

        if (stateMachine.CurrentState != null)
        {
            stateMachine.CurrentState.Exit();
        }
        this.enabled = false; 
        if (turretSensor != null) turretSensor.enabled = false; 

        Light foundLight = GetComponentInChildren<Light>();
        if (foundLight != null)
        {
            foundLight.gameObject.SetActive(false);
        }
    
        TurretLightController lightController = GetComponentInChildren<TurretLightController>();
        if (lightController != null)
        {
            lightController.enabled = false;
        }

        Debug.Log("[TURRET] Completely deactivated via terminal hacking.");
    }
}