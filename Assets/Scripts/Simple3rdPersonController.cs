using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Simple3rdPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;         
    public float rotationSpeed = 10f; 
    public float gravity = -9.81f;

    private CharacterController controller;
    private Vector3 velocity;
    private PlayerControls controls;
    private Transform mainCameraTransform;

    void Awake()
    {
        controls = new PlayerControls();
    }

    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();

    void Start()
    {
        controller = GetComponent<CharacterController>();
        
        // Find the actual camera lens in the world
        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        // 1. Gravity Setup
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // 2. Read WASD Input
        Vector2 moveInput = controls.Player.Move.ReadValue<Vector2>();
        
        // Create a clean input direction vector based purely on keys pressed
        Vector3 inputDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

        // 3. Move relative to the world-facing direction of the camera lens
        if (inputDirection.magnitude >= 0.1f && mainCameraTransform != null)
        {
            // Get the camera's current horizontal directions in the world
            Vector3 forward = mainCameraTransform.forward;
            Vector3 right = mainCameraTransform.right;
            
            // Flatten them completely so the player stays on the ground plane
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            // Calculate the EXACT direction the player wants to go relative to the screen
            Vector3 moveDirection = (forward * inputDirection.z) + (right * inputDirection.x);
            moveDirection.Normalize();

            // Move the player capsule straight along that line
            controller.Move(moveDirection * moveSpeed * Time.deltaTime);

            // Smoothly rotate the player's visual model to face the direction they are walking
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // 4. Apply Gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}