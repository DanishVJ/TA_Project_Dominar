using UnityEngine;

public class TurretLightController : MonoBehaviour
{
    private Light _spotLight;

    [Header("Arcade Color States")]
    [SerializeField] private Color patrolColor = Color.green;
    [SerializeField] private Color trackColor = Color.yellow;
    [SerializeField] private Color attackColor = Color.red;

    void Awake()
    {
        _spotLight = GetComponent<Light>();
        SetPatrolColor();
    }

    public void SetPatrolColor()
    {
        if (_spotLight != null) _spotLight.color = patrolColor;
    }

    public void SetTrackColor()
    {
        if (_spotLight != null) _spotLight.color = trackColor;
    }

    public void SetAttackColor()
    {
        if (_spotLight != null) _spotLight.color = attackColor;
    }
}