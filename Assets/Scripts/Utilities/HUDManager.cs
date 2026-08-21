using UnityEngine;
using TMPro;
using System.Collections;

public class HUDManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI interactPromptText;
    private Coroutine hideMessageCoroutine;

    private void OnEnable()
    {
        PlayerController.OnInteractableChanged += UpdateInteractPrompt;
        TurretManager.OnAllTurretsDisabled += HandleAllTurretsDisabled;
    }

    private void OnDisable()
    {
        PlayerController.OnInteractableChanged -= UpdateInteractPrompt;
        TurretManager.OnAllTurretsDisabled -= HandleAllTurretsDisabled;
    }

    private void UpdateInteractPrompt(string prompt)
    {
        // If an interact prompt overrides, we can handle it or let it take priority
        if (interactPromptText != null && string.IsNullOrEmpty(prompt) == false)
        {
            interactPromptText.text = prompt;
            interactPromptText.gameObject.SetActive(true);
        }
    }

    private void HandleAllTurretsDisabled()
    {
        DisplayTemporaryMessage("Now how to get outta here?", 5f);
    }
    
    public void DisplayTemporaryMessage(string message, float duration = 5f)
    {
        if (interactPromptText != null)
        {
            interactPromptText.text = message;
            interactPromptText.gameObject.SetActive(true);

            // If a previous timer is running, stop it so they don't conflict
            if (hideMessageCoroutine != null)
            {
                StopCoroutine(hideMessageCoroutine);
            }

            hideMessageCoroutine = StartCoroutine(HideMessageAfterDelay(duration));
        }
    }

    private IEnumerator HideMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (interactPromptText != null)
        {
            interactPromptText.text = string.Empty;
            interactPromptText.gameObject.SetActive(false);
        }

        hideMessageCoroutine = null;
    }
}