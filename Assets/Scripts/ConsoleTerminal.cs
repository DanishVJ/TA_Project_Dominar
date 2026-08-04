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
        Debug.Log("Terminal interacted successfully!");
        OnTerminalActivated?.Invoke();
    }

    public string GetInteractPrompt()
    {
        return promptMessage;
    }
}