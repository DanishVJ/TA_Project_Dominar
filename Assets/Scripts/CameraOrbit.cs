using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    public float mouseSensitivity = 15f; 
    private float xRotation = 0f;
    private float yRotation = 0f; // Tracking look rotation separately from player body
    private PlayerControls controls;

    void Awake()
    {
        controls = new PlayerControls();
    }

    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Match initial rotation of parent/player if nested
        yRotation = transform.localRotation.eulerAngles.y;
    }

    void Update()
    {
        // Read mouse/stick looking input
        Vector2 lookInput = controls.Player.Look.ReadValue<Vector2>();

        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        // Accumulate mouse X (looking around horizontally)
        yRotation += mouseX;

        // Accumulate mouse Y (looking around vertically) and clamp it
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -10f, 60f);

        // Apply clean rotation strictly to the camera boom/pivot object
        // The mouse controls this entirely, leaving the player body completely unaffected
        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
    }
}