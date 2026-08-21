using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class TurretManager : MonoBehaviour
{
    public static TurretManager Instance { get; private set; }

    [Header("Turret Tracking")]
    [SerializeField] private int totalTurrets = 3; 
    private int deactivatedTurrets = 0;

    // Event broadcast when all turrets are disabled
    public static event Action OnAllTurretsDisabled;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        // DEBUG KEY: Press 'T' to instantly test the message using the New Input System
        if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
        {
            Debug.Log("[TurretManager] DEBUG KEY PRESSED: Forcing All Turrets Disabled!");
            OnAllTurretsDisabled?.Invoke();
        }
    }

    public void TurretDeactivated()
    {
        deactivatedTurrets++;
        Debug.Log($"[TurretManager] Turret deactivated. Progress: {deactivatedTurrets}/{totalTurrets}");

        if (AreAllTurretsDisabled())
        {
            OnAllTurretsDisabled?.Invoke();
        }
    }

    public bool AreAllTurretsDisabled()
    {
        return deactivatedTurrets >= totalTurrets;
    }
}