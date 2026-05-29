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

    void Awake()
    {
        controls = new PlayerControls();
    }

    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();

    void Start()
    {
        controller = GetComponent<CharacterController>();
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
        
        // --- KEYBOARD STEERING (A and D keys only) ---
        // A and D rotate the player's body directly left and right
        if (Mathf.Abs(moveInput.x) > 0.1f)
        {
            float turnAmount = moveInput.x * rotationSpeed * Time.deltaTime;
            transform.Rotate(Vector3.up * turnAmount);
        }

        // --- FORWARD/BACKWARD MOVEMENT (W and S keys) ---
        // Calculate movement relative to the player's current forward facing direction
        Vector3 moveDirection = transform.forward * moveInput.y;

        // Move the player capsule
        if (moveDirection.magnitude >= 0.1f)
        {
            controller.Move(moveDirection * moveSpeed * Time.deltaTime);
        }

        // 3. Apply Gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}