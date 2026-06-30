using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Simple3rdPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 150f; // Controls how fast A/D spins the body

    [Header("Gravity Settings")]
    [SerializeField] private float gravityValue = -9.81f;
    // A small constant downward force when grounded keeps the player snapped firmly to stairs
    [SerializeField] private float groundedGripForce = -2f; 

    private CharacterController controller;
    private PlayerControls controls;
    private Vector2 moveInput;
    private Vector3 movementDirection;
    private Vector3 verticalVelocity; // Track vertical falling speed independently

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        controls = new PlayerControls();
    }

    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();

    void Update()
    {
        // 1. Read WASD / Stick Input
        moveInput = controls.Player.Move.ReadValue<Vector2>();

        // 2. Rotate the Player Body (A/D or Left/Right Stick)
        if (Mathf.Abs(moveInput.x) > 0.1f)
        {
            transform.Rotate(0f, moveInput.x * rotationSpeed * Time.deltaTime, 0f);
        }

        // 3. Grounded Check & Reset Vertical Velocity
        // If we are touching the ground/stairs, reset velocity so gravity doesn't infinitely build up
        if (controller.isGrounded && verticalVelocity.y < 0)
        {
            verticalVelocity.y = groundedGripForce;
        }

        // 4. Calculate Forward/Backward Movement (W/S or Up/Down Stick)
        movementDirection = transform.forward * moveInput.y;

        // 5. Apply Horizontal Movement
        controller.Move(movementDirection * moveSpeed * Time.deltaTime);

        // 6. Calculate Gravity over time (Acceleration)
        verticalVelocity.y += gravityValue * Time.deltaTime;

        // 7. Apply Vertical Movement (Falling / Descending Stairs)
        controller.Move(verticalVelocity * Time.deltaTime);
    }
}