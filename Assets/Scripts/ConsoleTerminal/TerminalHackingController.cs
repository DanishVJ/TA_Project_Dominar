using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TerminalHackingController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image leftSlotImage;
    [SerializeField] private Image rightSlotImage;

    [Header("Puzzle Sprites")]
    [SerializeField] private List<Sprite> availableSprites;

    private int leftIndex = 0;
    private int rightIndex = 0;
    private int activeSlot = 0; // 0 = Left Slot, 1 = Right Slot

    [Header("Winning Solution")]
    [SerializeField] private int winningLeftIndex = 2;
    [SerializeField] private int winningRightIndex = 5;
    
    [Header("UI Slot Scaling")]
    [SerializeField] private float activeScale = 1.15f;
    [SerializeField] private float normalScale = 1.0f;
    
    [Header("Audio Feedback")]
    [SerializeField] private AudioClip slotSwitchClip;
    [SerializeField] private AudioClip spriteCycleClip;
    [SerializeField] private AudioSource audioSource;
    
    private TurretController targetTurret;
    private ConsoleTerminal activeTerminal;

    void OnEnable()
    {
        leftIndex = 0;
        rightIndex = 0;
        
        activeSlot = 0;
        UpdateUI();
        
        InputManager.OnTerminalNavigate += HandleNavigate;
        InputManager.OnTerminalSubmit += HandleSubmit;
    }

    void OnDisable()
    {
        InputManager.OnTerminalNavigate -= HandleNavigate;
        InputManager.OnTerminalSubmit -= HandleSubmit;
    }

    private void HandleNavigate(Vector2 moveInput)
    {
        // Only process if we are currently in the TerminalHacking state
        if (GameStateManager.Instance == null || GameStateManager.Instance.CurrentState != GameState.TerminalHacking)
            return;

        // Horizontal input (Left/Right or A/D) cycles through active slots endlessly
        if (Mathf.Abs(moveInput.x) > 0.5f)
        {
            int direction = (moveInput.x > 0) ? 1 : -1;
            int totalSlots = 2; // Left and right slots
            
            activeSlot = (activeSlot + direction + totalSlots) % totalSlots;
            
            if (audioSource != null && slotSwitchClip != null)
                audioSource.PlayOneShot(slotSwitchClip);
            
            UpdateUI(); // Updates the slot scaling immediately upon switching slots
        }

        // Vertical input (Up/Down or W/S) cycles sprites
        if (Mathf.Abs(moveInput.y) > 0.5f)
        {
            int direction = (moveInput.y > 0) ? 1 : -1;
            ChangeSprite(direction);
        }
    }

    private void HandleSubmit()
    {
        if (GameStateManager.Instance == null || GameStateManager.Instance.CurrentState != GameState.TerminalHacking)
            return;

        CheckSolution();
    }

    private void ChangeSprite(int direction)
    {
        if (availableSprites == null || availableSprites.Count == 0) return;

        if (activeSlot == 0)
        {
            leftIndex = (leftIndex + direction + availableSprites.Count) % availableSprites.Count;
        }
        else
        {
            rightIndex = (rightIndex + direction + availableSprites.Count) % availableSprites.Count;
        }

        if (audioSource != null && spriteCycleClip != null)
            audioSource.PlayOneShot(spriteCycleClip);

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (availableSprites != null && availableSprites.Count > 0)
        {
            if (leftSlotImage != null) leftSlotImage.sprite = availableSprites[leftIndex];
            if (rightSlotImage != null) rightSlotImage.sprite = availableSprites[rightIndex];
        }
        
        // Pop the scale of the active slot
        if (leftSlotImage != null)
        {
            float targetScale = (activeSlot == 0) ? activeScale : normalScale;
            leftSlotImage.transform.localScale = new Vector3(targetScale, targetScale, 1f);
        }
    
        if (rightSlotImage != null)
        {
            float targetScale = (activeSlot == 1) ? activeScale : normalScale;
            rightSlotImage.transform.localScale = new Vector3(targetScale, targetScale, 1f);
        }
    }

    private void CheckSolution()
    {
        bool isSuccess = (leftIndex == winningLeftIndex && rightIndex == winningRightIndex);

        if (activeTerminal != null)
        {
            // Pass the solution result to the terminal to handle the HUD message, sounds, and delay
            activeTerminal.HandleHackingResult(isSuccess, targetTurret);
        }
        else
        {
            Debug.LogWarning("[HACKING] No active terminal reference found!");
            CloseTerminal();
        }
    }

    private void CloseTerminal()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SetState(GameState.Playing);
        }
    }
    
    public void SetTargetTurret(TurretController turret)
    {
        targetTurret = turret;
        Debug.Log($"[HACKING] Target turret successfully set to: {turret.gameObject.name}");
    }

    public void SetActiveTerminal(ConsoleTerminal terminal)
    {
        activeTerminal = terminal;
    }
    
    public void SetWinningCombination(int left, int right)
    {
        winningLeftIndex = left;
        winningRightIndex = right;
        Debug.Log($"[HACKING] Winning combination updated to: {left} and {right}");
    }
}