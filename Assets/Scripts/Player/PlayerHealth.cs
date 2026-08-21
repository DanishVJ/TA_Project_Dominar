using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;
    private bool isDead = false; // Prevents the death sequence from running multiple times

    [Header("Damage Effects")]
    [SerializeField] private GameObject smokeParticlePrefab; // Drag your smoke prefab here in Inspector
    [SerializeField] private Transform smokeSpawnPoint;      // Where on the player the smoke appears (e.g., chest/spine)

    // Events for UI and other systems
    public static event Action<float, float> OnHealthChanged; // Passes (currentHealth, maxHealth)
    public static event Action OnPlayerDied;

    private void Start()
    {
        currentHealth = maxHealth;
        // Broadcast initial health so the UI populates right away when the game starts
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        
        Debug.Log($"[PLAYER] Hit! Health remaining: {currentHealth}/{maxHealth}");

        // Spawn damage smoke effect
        TriggerDamageSmoke();

        // Tell listeners (like your HUD) that health changed
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void TriggerDamageSmoke()
    {
        if (smokeParticlePrefab != null)
        {
            Transform spawnParent = smokeSpawnPoint != null ? smokeSpawnPoint : transform;
            GameObject smoke = Instantiate(smokeParticlePrefab, spawnParent.position, spawnParent.rotation, spawnParent);
            
            // Destroy the smoke effect after a few seconds so it doesn't clutter the hierarchy
            Destroy(smoke, 3f);
        }
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("[PLAYER] You died! Triggering Game Over screen...");
        
        // Notify any listeners that the player has died
        OnPlayerDied?.Invoke();

        // Tell our GameStateManager to switch to the GameOver state instantly
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SetState(GameState.GameOver);
        }
    }

    // Public getters if needed
    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
}