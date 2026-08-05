using UnityEngine;
using System;

public class ConsoleTerminal : MonoBehaviour, IInteractable
{
    [Header("Prompt Settings")]
    [SerializeField] private string promptMessage = "Press E to Use Terminal";

    // Event that notifies listeners when the terminal is opened
    public static event Action OnTerminalActivated;

    public void Interact()
    {
        Debug.Log("Terminal activated successfully!");
        
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SetState(GameState.TerminalHacking);
        }
        
        OnTerminalActivated?.Invoke();
    }

    public string GetInteractPrompt()
    {
        return promptMessage;
    }
}