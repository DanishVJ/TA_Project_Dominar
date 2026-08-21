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
    [SerializeField] private AudioClip successClip;

    [Header("Escape Terminal Settings")]
    [SerializeField] private bool isExitTerminal = false;
    public bool IsExitTerminal => isExitTerminal;

    [Header("Escape Environment Swap")]
    [SerializeField] private GameObject[] solidDomePanels;     // Drag multiple solid dome panels here
    [SerializeField] private GameObject[] doorwayDomePanels;   // Drag multiple doorway dome variants here
    [SerializeField] private GameObject exitPointLight;     // Drag point light / parent light object here
    [SerializeField] private GameObject exitTriggerVolume;  // Drag win trigger box / parent object here
    
    // Event that notifies listeners when the terminal is opened
    public static event Action OnTerminalActivated;

    public void Interact()
    {
        // Check if this is the exit terminal
        if (isExitTerminal)
        {
            if (TurretManager.Instance != null && !TurretManager.Instance.AreAllTurretsDisabled())
            {
                Debug.Log("[TERMINAL] Access Denied: Active turrets remain online.");
                PlayIncorrectSound();
                
                HUDManager hud = FindObjectOfType<HUDManager>();
                if (hud != null) hud.DisplayTemporaryMessage("Access Denied: Turrets Active!");

                return; // Stop here, do not open hacking UI
            }
            else
            {
                // Turrets are defeated! Trigger escape sequence, play success sound, and show message.
                PlaySuccessSound();

                HUDManager hud = FindObjectOfType<HUDManager>();
                if (hud != null) hud.DisplayTemporaryMessage("Escape Doors Open!");

                TriggerEscapeSequence();
                return;
            }
        }

        Debug.Log("Terminal activated successfully!");
    
        // Find the hacking controller (including inactive/instantiated clones)
        TerminalHackingController[] hackingControllers = FindObjectsOfType<TerminalHackingController>(true);
        if (hackingControllers.Length > 0)
        {
            // Pass the terminal reference so it can trigger audio feedback
            hackingControllers[0].SetActiveTerminal(this);

            // Pass the turret reference (if assigned)
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
            terminalAudioSource.clip = incorrectClip;
            terminalAudioSource.Play();
        }
    }

    public void PlaySuccessSound()
    {
        if (terminalAudioSource != null && successClip != null)
        {
            terminalAudioSource.clip = successClip;
            terminalAudioSource.Play();
        }
    }

    public void TriggerEscapeSequence()
    {
        // Turn off all solid dome panels
        if (solidDomePanels != null)
        {
            foreach (GameObject panel in solidDomePanels)
            {
                if (panel != null) panel.SetActive(false);
            }
        }

        // Turn on all doorway dome panels
        if (doorwayDomePanels != null)
        {
            foreach (GameObject panel in doorwayDomePanels)
            {
                if (panel != null) panel.SetActive(true);
            }
        }

        if (exitPointLight != null) exitPointLight.SetActive(true);
        if (exitTriggerVolume != null) exitTriggerVolume.SetActive(true);
        
        Debug.Log("[ESCAPE] Hangar airlocks opened! Escape routes active.");
    }
}