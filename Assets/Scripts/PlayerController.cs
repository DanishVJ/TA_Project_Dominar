using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 100f; 
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpVelocity = 5f;

    [Space(10)]
    [Header("Ground Check")]
    [SerializeField] private Vector3 groundCheckOffset;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private float groundCheckRadius = 0.3f;
    [SerializeField] private LayerMask groundLayer;

    [Space(10)]
    [Header("Interaction Detection")]
    [SerializeField] private float interactionCheckRadius = 2f;
    [SerializeField] private LayerMask interactableLayer;

    public event Action OnJumpEvent;
    
    public Vector2 moveInput;
    private Vector3 _moveDirection;
    private CharacterController _characterController;
    private Vector3 _velocity;
    private bool _isGrounded;

    private IInteractable _currentInteractable;

    public bool IsGrounded() => _isGrounded;
    public Vector3 GetPlayerVelocity() => _velocity;
    
    void Start()
    {
        _characterController = GetComponent<CharacterController>();

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        var controller = InputManager.Instance;
    }
    
    void Update()
    {
        CalculateMovementExplore();
        _characterController.Move(_velocity * Time.deltaTime);
        CheckForInteractables();
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
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump()
    {
        if(_isGrounded)
        {
            _velocity.y = jumpVelocity;
        }
    }

    public void OnInteract()
    {
        if (_currentInteractable != null)
        {
            _currentInteractable.Interact();
        }
    }

    private void CheckForInteractables()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactionCheckRadius, interactableLayer);
        
        IInteractable closestInteractable = null;
        float closestDistance = float.MaxValue;

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IInteractable>(out var interactable))
            {
                float distance = Vector3.Distance(transform.position, hit.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestInteractable = interactable;
                }
            }
        }

        // Log only when the interaction target changes
        if (_currentInteractable != closestInteractable)
        {
            _currentInteractable = closestInteractable;

            if (_currentInteractable != null)
            {
                Debug.Log($"Detected interactable: {(_currentInteractable as MonoBehaviour)?.gameObject.name}");
            }
            else
            {
                Debug.Log("Left interaction range.");
            }
        }
    }

    private void CalculateMovementExplore()
    {
        Vector3 cameraForward = playerCamera.transform.forward;
        Vector3 cameraRight = playerCamera.transform.right;
        
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        
        cameraForward.Normalize();
        cameraRight.Normalize();
        
        Vector3 moveDirection = (cameraForward * moveInput.y + cameraRight * moveInput.x).normalized;

        if (moveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        _velocity = (Vector3.up * _velocity.y) + (moveDirection * moveSpeed);
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
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionCheckRadius);
    }
}