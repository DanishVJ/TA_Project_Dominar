using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;
    private bool isDead = false; // Prevents the death sequence from running multiple times

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        Debug.Log($"[PLAYER] Hit! Health remaining: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("[PLAYER] You died! Triggering Game Over screen...");
        
        // Tell our GameStateManager to switch to the GameOver state instantly
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SetState(GameState.GameOver);
        }
    }
}