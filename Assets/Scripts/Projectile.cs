using UnityEngine;

public class Projectile : MonoBehaviour
{
    private float _damage;

    public void Initialize(float damage)
    {
        _damage = damage;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check if we hit the player and deal damage
        if (collision.gameObject.TryGetComponent<PlayerHealth>(out PlayerHealth playerHealth))
        {
            playerHealth.TakeDamage(_damage);
        }

        // Destroy the bullet instantly upon hitting anything
        Destroy(gameObject);
    }
}