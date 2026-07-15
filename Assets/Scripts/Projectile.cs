using UnityEngine;

public class Projectile : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        // Destroy the bullet instantly upon hitting anything (player, wall, floor)
        Destroy(gameObject);
    }
}