using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 100f; // Note: Boosted default value for snappier in-place tank turning
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
    private Vector3 _moveDirection;
    private CharacterController _characterController;
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

    void OnEnable()
    {
        _controls.Enable();
        ConfigureMouseForVM();
    }

    void OnDisable()
    {
        _controls.Disable();
    }
    
    void Start()
    {
        _characterController = GetComponent<CharacterController>();

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        ConfigureMouseForVM();
    }

    private void ConfigureMouseForVM()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            ConfigureMouseForVM();
        }
    }
    
    void Update()
    {
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

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
        // 1. ABSOLUTE TANK ROTATION: A/D rotates the player body directly around world Y-axis
        if (Mathf.Abs(_moveInput.x) > 0.01f)
        {
            float rotationAmount = _moveInput.x * rotationSpeed * Time.deltaTime;
            transform.Rotate(0f, rotationAmount, 0f);
        }

        // 2. ABSOLUTE TANK MOVEMENT: W/S moves purely along the player's own local forward direction
        if (Mathf.Abs(_moveInput.y) > 0.01f)
        {
            _moveDirection = transform.forward * _moveInput.y;
        }
        else
        {
            _moveDirection = Vector3.zero;
        }

        // 3. Update final velocity components and apply gravity
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