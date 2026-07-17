using UnityEngine;

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

    // EXPOSED CENTER MASS PROPERTY: Allows states to track target center instead of feet pivot
    public Vector3 TargetCenterPosition { get; private set; }

    private void Update()
    {
        FindVisibleTarget();
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
                    TargetCenterPosition = targetCenterPos; // Store center mass location
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 0.5f, 1f, 0.15f);
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        Gizmos.color = Color.yellow;
        
        Vector3 forward = transform.forward;
        Vector3 up = transform.up;
        Vector3 right = transform.right;

        Vector3 leftCone = Quaternion.AngleAxis(-viewAngle / 2f, up) * forward;
        Vector3 rightCone = Quaternion.AngleAxis(viewAngle / 2f, up) * forward;
        Vector3 upCone = Quaternion.AngleAxis(-viewAngle / 2f, right) * forward;
        Vector3 downCone = Quaternion.AngleAxis(viewAngle / 2f, right) * forward;

        Gizmos.DrawLine(transform.position, transform.position + leftCone * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + rightCone * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + upCone * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + downCone * viewRadius);

        Vector3 endCenter = transform.position + forward * viewRadius;
        float radiusAtEnd = viewRadius * Mathf.Tan(viewAngle / 2f * Mathf.Deg2Rad);
        
#if UNITY_EDITOR
        UnityEditor.Handles.color = Color.yellow;
        UnityEditor.Handles.DrawWireDisc(endCenter, forward, radiusAtEnd);
#endif
    }
}