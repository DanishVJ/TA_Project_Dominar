using UnityEngine;
using System;

public class ConsoleTerminal : MonoBehaviour, IInteractable
{
    [Header("Prompt Settings")]
    [SerializeField] private string promptMessage = "Press E to Use Terminal";

    [Header("Turret Assignment")]
    [SerializeField] private TurretController associatedTurret;
    
    [Header("Puzzle Winning Solution")]
    [SerializeField] private int terminalWinningLeft = 2;
    [SerializeField] private int terminalWinningRight = 5;

    [Header("Audio Feedback")]
    [SerializeField] private AudioSource terminalAudioSource;
    [SerializeField] private AudioClip incorrectClip;
    
    // Event that notifies listeners when the terminal is opened
    public static event Action OnTerminalActivated;

    public void Interact()
    {
        Debug.Log("Terminal activated successfully!");
    
        // Find the hacking controller (including inactive/instantiated clones)
        TerminalHackingController[] hackingControllers = FindObjectsOfType<TerminalHackingController>(true);
        if (hackingControllers.Length > 0)
        {
            // Pass the terminal reference so it can trigger audio feedback
            hackingControllers[0].SetActiveTerminal(this);

            // Pass the turret reference
            if (associatedTurret != null)
            {
                hackingControllers[0].SetTargetTurret(associatedTurret);
            }

            // Pass the winning combination
            hackingControllers[0].SetWinningCombination(terminalWinningLeft, terminalWinningRight);
        }
        else
        {
            Debug.LogWarning("[TERMINAL] Could not find TerminalHackingController!");
        }

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

    public void PlayIncorrectSound()
    {
        if (terminalAudioSource != null && incorrectClip != null)
        {
            terminalAudioSource.PlayOneShot(incorrectClip);
        }
    }
}