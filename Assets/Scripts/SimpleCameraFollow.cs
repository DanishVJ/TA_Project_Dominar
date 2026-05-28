using UnityEngine;

public class SimpleCameraFollow : MonoBehaviour
{
    public Transform target;          // Drag your Player capsule here
    public float distance = 5.0f;     // How far behind the player the camera sits
    public float height = 2.5f;       // How high up the camera sits
    public float rotationSpeed = 5.0f; // How fast the camera catches up to the player

    void LateUpdate()
    {
        if (target == null) return;

        // 1. Calculate the exact position behind the player's current facing direction
        Vector3 targetPosition = target.position - (target.forward * distance) + (Vector3.up * height);

        // 2. Smoothly move the camera to that ideal spot
        transform.position = Vector3.Lerp(transform.position, targetPosition, rotationSpeed * Time.deltaTime);

        // 3. Force the camera to look directly at the player's neck/head area
        transform.LookAt(target.position + (Vector3.up * 1.5f));
    }
}