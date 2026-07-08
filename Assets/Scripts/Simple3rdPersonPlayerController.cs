using System;
using UnityEngine;
using UnityEngine.InputSystem;

// ...existing code...
public class PlayerController : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpVelocity = 5f;

    [Space(10)]
    [Header("Ground Check")]
    [SerializeField] private Vector3 groundCheckOffset;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private float groundCheckRadius = 0.3f;
    [SerializeField] private LayerMask groundLayer;

    public event Action OnJumpEvent;

    // Input
    private Vector2 _moveInput;
    private PlayerControls _controls;

    // Movement / physics
    private CharacterController _characterController;
    private Vector3 _velocity;
    private bool _isGrounded;

    // State machine (reuse small engine already present in project)
    public TurretStateMachine StateMachine { get; private set; }
    public IState IdleState { get; private set; }
    public IState MoveState { get; private set; }
    public IState JumpState { get; private set; }
    public IState FallState { get; private set; }

    // Expose a few members to states
    public Camera PlayerCamera => playerCamera;
    public CharacterController CharacterController => _characterController;
    public float MoveSpeed => moveSpeed;
    public float RotationSpeed => rotationSpeed;
    public float Gravity => gravity;
    public float JumpVelocity => jumpVelocity;
    public Vector2 MoveInput => _moveInput;
    public Vector3 Velocity { get => _velocity; set => _velocity = value; }

    public bool IsGrounded() => _isGrounded;
    public Vector3 GetPlayerVelocity() => _velocity;

    void Awake()
    {
        _controls = new PlayerControls();

        _controls.Player.Move.performed += OnMove;
        _controls.Player.Move.canceled += OnMove;

        _controls.Player.Jump.performed += _ => OnJump();

        // Create state machine and states (reuse TurretStateMachine implementation)
        StateMachine = new TurretStateMachine();
        IdleState = new PlayerIdleState(this);
        MoveState = new PlayerMoveState(this);
        JumpState = new PlayerJumpState(this);
        FallState = new PlayerFallState(this);
    }

    void OnEnable() => _controls.Enable();
    void OnDisable() => _controls.Disable();

    void Start()
    {
        _characterController = GetComponent<CharacterController>();
        StateMachine.Initialize(IdleState);
    }

    void Update()
    {
        StateMachine.ExecuteActiveState();
    }

    private void FixedUpdate()
    {
        CheckGrounded();

        if (_isGrounded && _velocity.y < 0)
        {
            _velocity.y = -0.2f;
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump()
    {
        Debug.Log($"PlayerController.OnJump called. isGrounded={_isGrounded}");
        if (_isGrounded)
        {
            // Let the state machine handle the actual jump behaviour
            StateMachine.ChangeState(JumpState);
        }
    }

    // Allow states to trigger the public jump event since events can only be invoked
    // from within the declaring type.
    public void RaiseJumpEvent()
    {
        Debug.Log("PlayerController.RaiseJumpEvent invoked");
        OnJumpEvent?.Invoke();
    }

    private void CheckGrounded()
    {
        // Use SphereCast like original implementation: cast a sphere downward to detect ground within a small distance.
        // Use discard for the out param to avoid an unused variable warning.
        _isGrounded = Physics.SphereCast(transform.position + groundCheckOffset, groundCheckRadius, Vector3.down, out RaycastHit _, groundCheckDistance, groundLayer);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.purple;
        Gizmos.DrawSphere(transform.position + groundCheckOffset, groundCheckRadius);
        Gizmos.DrawSphere(transform.position + groundCheckOffset + Vector3.down * groundCheckDistance, groundCheckRadius);
        Gizmos.DrawCube(transform.position + groundCheckOffset + Vector3.down * groundCheckDistance / 2,
            new Vector3(1.5f * groundCheckRadius, groundCheckDistance, 1.5f * groundCheckRadius));
    }
}