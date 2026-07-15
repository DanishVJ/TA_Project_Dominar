using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileAttack", menuName = "Turret/Strategies/Projectile Attack")]
public class ProjectileAttackStrategy : ScriptableObject, IAttackStrategy
{
    [SerializeField] private float damage = 10f;

    public void Attack(Transform firePoint, GameObject projectilePrefab, float projectileSpeed, float damageOverride)
    {
        if (projectilePrefab == null || firePoint == null) return;

        // Spawn bullet
        GameObject bullet = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        
        // Pass damage to the projectile
        if (bullet.TryGetComponent<Projectile>(out Projectile proj))
        {
            proj.Initialize(damage);
        }

        // Shoot it forward
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = firePoint.forward * projectileSpeed;
        }

        Destroy(bullet, 5f);
    }
}