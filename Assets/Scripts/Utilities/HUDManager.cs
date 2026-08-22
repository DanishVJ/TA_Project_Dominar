using UnityEngine;
using TMPro;
using System.Collections;

public class HUDManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI interactPromptText;
    private Coroutine hideMessageCoroutine;
    private bool isTemporaryMessageActive = false; // Blocks raycasts completely until the timer ends

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
        if (interactPromptText != null)
        {
            // If a temporary message is currently showing, completely ignore standard prompts!
            if (isTemporaryMessageActive) return;

            if (!string.IsNullOrEmpty(prompt))
            {
                interactPromptText.text = prompt;
                interactPromptText.gameObject.SetActive(true);
            }
            else
            {
                interactPromptText.text = string.Empty;
                interactPromptText.gameObject.SetActive(false);
            }
        }
    }

    private void HandleAllTurretsDisabled()
    {
        DisplayTemporaryMessage("Now how to get outta here?", 5f);
    }
    
    public void DisplayTemporaryMessage(string message, float duration = 3f)
    {
        if (interactPromptText != null)
        {
            if (hideMessageCoroutine != null)
            {
                StopCoroutine(hideMessageCoroutine);
            }

            isTemporaryMessageActive = true; // Lock out standard prompts immediately
            interactPromptText.text = message;
            interactPromptText.gameObject.SetActive(true);

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

        isTemporaryMessageActive = false; // Allow standard prompts again
        hideMessageCoroutine = null;
    }
}