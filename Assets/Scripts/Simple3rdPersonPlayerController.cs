using System;
using UnityEngine;
using UnityEngine.InputSystem;

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
    
    private Vector2 _moveInput;
    private Vector3 _camForward;
    private Vector3 _camRight;
    private Vector3 _moveDirection;
    private CharacterController _characterController;
    private Quaternion _targetRotation;
    private Vector3 _velocity;
    private bool _isGrounded;
    private PlayerControls _controls;

    public bool IsGrounded() => _isGrounded;
    public Vector3 GetPlayerVelocity() => _velocity;
    
    void Awake()
    {
        _controls = new PlayerControls();
        
        _controls.Player.Move.performed += OnMove;
        _controls.Player.Move.canceled += OnMove;
        
        _controls.Player.Jump.performed += ctx => OnJump();
    }

    void OnEnable() => _controls.Enable();
    void OnDisable() => _controls.Disable();
    
    void Start()
    {
        _characterController = GetComponent<CharacterController>();

        // Fix: Locks cursor to center screen so cursor offset doesn't cause constant drift
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    void Update()
    {
        CalculateMovementExplore();
        _characterController.Move(_velocity * Time.deltaTime);
    }

    private void FixedUpdate()
    {
        CheckGrounded();
        
        if(_isGrounded && _velocity.y < 0)
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
        if(_isGrounded)
        {
            _velocity.y = jumpVelocity;
            OnJumpEvent?.Invoke(); 
        }
    }

    private void CalculateMovementExplore()
    {
        // 1. Grab raw WASD / Arrow Key input
        Vector3 inputDirection = new Vector3(_moveInput.x, 0f, _moveInput.y);

        // 2. Only run movement & rotation math if there is active input
        if (inputDirection.sqrMagnitude > 0.01f)
        {
            // Get the camera's flat angle on the horizon (Y-axis/yaw only)
            float cameraYaw = playerCamera.transform.eulerAngles.y;
            
            // Convert camera angle to a rotation matrix
            Quaternion cameraRotation = Quaternion.Euler(0f, cameraYaw, 0f);

            // Rotate our WASD input direction relative to the camera's viewing angle
            // Left (A) and Right (D) now dynamically translate to world-space left/right relative to the screen
            _moveDirection = cameraRotation * inputDirection;

            // 3. Smoothly rotate the player's model to face the direction they are walking
            _targetRotation = Quaternion.LookRotation(_moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            // When not pressing keys, movement drops to zero (preventing continuous drift)
            _moveDirection = Vector3.zero;
        }

        // 4. Update velocity and apply gravity
        _velocity = (Vector3.up * _velocity.y) + (_moveDirection * moveSpeed);
        _velocity.y += gravity * Time.deltaTime;
    }

    private void CheckGrounded()
    {
        _isGrounded = Physics.SphereCast(transform.position + groundCheckOffset, groundCheckRadius, Vector3.down, out RaycastHit hit, groundCheckDistance, groundLayer);
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