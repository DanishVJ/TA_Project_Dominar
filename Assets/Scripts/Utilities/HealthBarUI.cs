using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private GameObject healthBarVisualRoot;

    private void Awake()
    {
        if (healthSlider == null)
        {
            healthSlider = GetComponent<Slider>();
        }

        if (healthBarVisualRoot == null)
        {
            healthBarVisualRoot = gameObject;
        }
    }

    private void Start()
    {
        CheckVisibility();
    }

    private void OnEnable()
    {
        PlayerHealth.OnHealthChanged += UpdateHealthBar;
    }

    private void OnDisable()
    {
        PlayerHealth.OnHealthChanged -= UpdateHealthBar;
    }

    private void Update()
    {
        // Continuously verify visibility based on the active game state
        CheckVisibility();
    }

    private void CheckVisibility()
    {
        if (GameStateManager.Instance != null)
        {
            // Only show if the current state is Playing
            bool isPlaying = (GameStateManager.Instance.CurrentState == GameState.Playing);
            if (healthBarVisualRoot.activeSelf != isPlaying)
            {
                healthBarVisualRoot.SetActive(isPlaying);
            }
        }
    }

    private void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth / maxHealth;
        }
    }
}