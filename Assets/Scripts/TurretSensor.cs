using UnityEngine;
using System.Collections;

public class TurretSensor : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private LayerMask targetMask;        // Set to your "Player" layer
    [SerializeField] private LayerMask obstructionMask;   // Set to your "Default" / "Wall" layer
    [SerializeField] private float viewRadius = 10f;       // How far the turret can see
    [Range(0, 360)]
    [SerializeField] private float viewAngle = 60f;        // The total width of the vision cone

    // Public properties so our states or controller can read these values if needed
    public float ViewRadius => viewRadius;
    public float ViewAngle => viewAngle;

    // This will store the player's transform when detected
    private Transform _detectedTarget;
    public Transform DetectedTarget => _detectedTarget;

    private void Start()
    {
        StartCoroutine(FindTargetsWithDelay(0.2f)); 
    }
    
    private IEnumerator FindTargetsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTarget();
        }
    }
    
    private void FindVisibleTarget()
    {
        // 1. Find all colliders on the Player layer within our view radius
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

        if (targetsInViewRadius.Length > 0)
        {
            // We found a player collider! Let's get its transform
            Transform target = targetsInViewRadius[0].transform.root;
            Vector3 directionToTarget = (target.position - transform.position).normalized;

            // 2. Check if the player is within our forward vision cone angle
            if (Vector3.Angle(transform.forward, directionToTarget) < viewAngle / 2)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);

                // 3. Raycast to ensure there are no walls (obstructions) between the turret and the player
                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstructionMask))
                {
                    // Line of sight is clear! We see the player
                    _detectedTarget = target;
                    Debug.Log($"[SENSOR] Target spotted: {_detectedTarget.name}");
                    return; 
                }
            }
        }

        // If any check fails, the turret loses track of the target
        if (_detectedTarget != null)
        {
            Debug.Log("[SENSOR] Target lost.");
        }
        _detectedTarget = null;
    }
}