using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Simple3rdPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;         
    public float rotationSpeed = 100f; // Adjusted for snappy keyboard steering
    public float gravity = -9.81f;

    private CharacterController controller;
    private Vector3 velocity;
    private PlayerControls controls;
    private Transform cameraTransform;

    void Awake()
    {
        controls = new PlayerControls();
    }

    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();

    void Start()
    {
        controller = GetComponent<CharacterController>();
        
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
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
        
        // 3. Calculate Camera-Relative Movement
        Vector3 movementDirection = Vector3.zero;
        
        if (moveInput.magnitude > 0.1f && cameraTransform != null)
        {
            // Get camera directions
            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight = cameraTransform.right;

            // Flatten them on the Y axis so walking doesn't make you fly or dig into the ground
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            // Combine camera direction with our raw WASD input
            movementDirection = (camForward * moveInput.y) + (camRight * moveInput.x);
            
            // Move the player capsule
            controller.Move(movementDirection * moveSpeed * Time.deltaTime);

            // Smoothly rotate the player's body to face the movement direction
            Quaternion targetRotation = Quaternion.LookRotation(movementDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // 4. Apply Gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}