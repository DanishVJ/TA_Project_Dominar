using UnityEngine;

public class TurretManager : MonoBehaviour
{
    public static TurretManager Instance { get; private set; }

    [Header("Turret Tracking")]
    [SerializeField] private int totalTurrets = 3; // Change this if you have a different amount
    private int deactivatedTurrets = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void TurretDeactivated()
    {
        deactivatedTurrets++;
        Debug.Log($"[TurretManager] Turret deactivated. Progress: {deactivatedTurrets}/{totalTurrets}");
    }

    public bool AreAllTurretsDisabled()
    {
        return deactivatedTurrets >= totalTurrets;
    }
}