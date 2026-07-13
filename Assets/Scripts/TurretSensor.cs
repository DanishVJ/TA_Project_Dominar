using UnityEngine;
using System.Collections;

public class TurretSensor : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private LayerMask targetMask;        // Set to your "Player" layer
    [SerializeField] private LayerMask obstructionMask;   // Set to your "Default" / "Wall" layer
    [SerializeField] private float viewRadius = 15f;       // Turn this up to increase visual distance!
    [Range(0, 360)]
    [SerializeField] private float viewAngle = 60f;        // The total width of the vision cone

    public float ViewRadius => viewRadius;
    public float ViewAngle => viewAngle;

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
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

        if (targetsInViewRadius.Length > 0)
        {
            Collider playerCollider = targetsInViewRadius[0];
            Transform target = playerCollider.transform;
            
            // Aim at the bounds center (usually chest height) rather than the root feet pivot
            Vector3 targetCenterPos = playerCollider.bounds.center;
            Vector3 directionToTarget = (targetCenterPos - transform.position).normalized;

            // Check if the target is within our forward vision cone angle
            if (Vector3.Angle(transform.forward, directionToTarget) < viewAngle / 2)
            {
                float distanceToTarget = Vector3.Distance(transform.position, targetCenterPos);

                // Raycast to ensure there are no walls between the turret and the player center
                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstructionMask))
                {
                    _detectedTarget = target.root; // Lock on to root object
                    return; 
                }
            }
        }

        if (_detectedTarget != null)
        {
            Debug.Log("[SENSOR] Target lost.");
        }
        _detectedTarget = null;
    }

    // This will draw the visual guides directly in your Unity Scene window
    private void OnDrawGizmosSelected()
    {
        // Draw the main detection radius sphere in light blue
        Gizmos.color = new Color(0, 0.5f, 1f, 0.15f);
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        // Draw the main 3D vision spotlight cone
        Gizmos.color = Color.yellow;
        
        // Use transform.rotation as the clean baseline direction
        Vector3 forward = transform.forward;
        Vector3 up = transform.up;
        Vector3 right = transform.right;

        // Calculate the 4 edge directions accurately relative to the barrel orientation
        Vector3 leftCone = Quaternion.AngleAxis(-viewAngle / 2f, up) * forward;
        Vector3 rightCone = Quaternion.AngleAxis(viewAngle / 2f, up) * forward;
        Vector3 upCone = Quaternion.AngleAxis(-viewAngle / 2f, right) * forward;
        Vector3 downCone = Quaternion.AngleAxis(viewAngle / 2f, right) * forward;

        // Draw the four outer guidelines
        Gizmos.DrawLine(transform.position, transform.position + leftCone * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + rightCone * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + upCone * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + downCone * viewRadius);

        // Draw a ring connecting the ends to show the true circular cone opening
        Vector3 endCenter = transform.position + forward * viewRadius;
        float radiusAtEnd = viewRadius * Mathf.Tan(viewAngle / 2f * Mathf.Deg2Rad);
        
        // This draws a helper circle at the target distance
#if UNITY_EDITOR
        UnityEditor.Handles.color = Color.yellow;
        UnityEditor.Handles.DrawWireDisc(endCenter, forward, radiusAtEnd);
#endif
    }
}