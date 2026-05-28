using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    public float mouseSensitivity = 15f; 
    private float xRotation = 0f;
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
    }

    void Update()
    {
        Vector2 lookInput = controls.Player.Look.ReadValue<Vector2>();

        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        // Rotate the boom horizontally (Left/Right)
        transform.Rotate(Vector3.up * mouseX);

        // Tilt the boom vertically (Up/Down) and clamp it
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -10f, 60f);
        transform.localRotation = Quaternion.Euler(xRotation, transform.localRotation.eulerAngles.y, 0f);
    }
}