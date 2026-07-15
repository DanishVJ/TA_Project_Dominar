using UnityEngine;
using UnityEngine.SceneManagement; // <-- Added this to allow reloading scenes

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
        Debug.Log("[PLAYER] You died! Restarting level in 2 seconds...");
        
        // Reloads the level after a 2-second delay
        Invoke(nameof(RestartLevel), 2f);
    }

    private void RestartLevel()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.name);
    }
}