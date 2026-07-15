using UnityEngine;

public interface IAttackStrategy
{
    void Attack(Transform firePoint, GameObject projectilePrefab, float projectileSpeed, float damageOverride);
}