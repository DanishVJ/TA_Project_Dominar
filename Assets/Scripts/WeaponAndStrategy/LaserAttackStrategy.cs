using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "LaserAttack", menuName = "Turret/Strategies/Laser Attack")]
public class LaserAttackStrategy : ScriptableObject, IAttackStrategy
{
    [SerializeField] private float damage = 15f;
    [SerializeField] private float maxLaserDistance = 50f;
    [SerializeField] private float beamDuration = 0.08f; 

    public void Attack(Transform firePoint, GameObject projectilePrefab, float projectileSpeed, float damageOverride)
    {
        if (firePoint == null) return;

        LineRenderer line = firePoint.GetComponent<LineRenderer>();
        if (line == null)
        {
            Debug.LogWarning($"[LASER] Missing LineRenderer component on {firePoint.name}!");
            return;
        }

        Vector3 origin = firePoint.position;
        Vector3 direction = firePoint.forward;
        Vector3 targetEndPosition = origin + (direction * maxLaserDistance);

        // 1. Enable the Line Renderer
        line.enabled = true;
        
        // 2. Because Use World Space is UNCHECKED, the starting point (origin) is always LOCAL (0,0,0)
        line.SetPosition(0, Vector3.zero);

        // 3. Cast the ray in global world space to find what we hit
        Ray ray = new Ray(origin, direction);
        if (Physics.Raycast(ray, out RaycastHit hit, maxLaserDistance))
        {
            targetEndPosition = hit.point;

            if (hit.collider.TryGetComponent<PlayerHealth>(out PlayerHealth player))
            {
                player.TakeDamage(damage);
            }

            Debug.DrawLine(origin, hit.point, Color.red, 0.1f);
        }
        else
        {
            Debug.DrawRay(origin, direction * maxLaserDistance, Color.red, 0.1f);
        }

        // 4. THE FIX: Convert the global targetEndPosition into local coordinates relative to the firePoint!
        Vector3 localEndPosition = firePoint.InverseTransformPoint(targetEndPosition);
        line.SetPosition(1, localEndPosition);

        // 5. Run the disable coroutine
        MonoBehaviour turretRunner = firePoint.GetComponentInParent<MonoBehaviour>();
        if (turretRunner != null && turretRunner.gameObject.activeInHierarchy)
        {
            turretRunner.StartCoroutine(DisableLaserAfterDelay(line, beamDuration));
        }
    }

    private IEnumerator DisableLaserAfterDelay(LineRenderer line, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (line != null)
        {
            line.enabled = false;
        }
    }
}