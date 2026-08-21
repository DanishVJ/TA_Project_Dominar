using UnityEngine;
using TMPro;

public class HUDManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI interactPromptText;

    private void OnEnable()
    {
        PlayerController.OnInteractableChanged += UpdateInteractPrompt;
    }

    private void OnDisable()
    {
        PlayerController.OnInteractableChanged -= UpdateInteractPrompt;
    }

    private void UpdateInteractPrompt(string prompt)
    {
        if (interactPromptText != null)
        {
            interactPromptText.text = prompt;
            interactPromptText.gameObject.SetActive(!string.IsNullOrEmpty(prompt));
        }
    }
    
    public void DisplayTemporaryMessage(string message)
    {
        if (interactPromptText != null)
        {
            interactPromptText.text = message;
            interactPromptText.gameObject.SetActive(true);
        }
    }
}