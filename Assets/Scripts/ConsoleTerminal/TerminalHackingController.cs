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

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (availableSprites != null && availableSprites.Count > 0)
        {
            if (leftSlotImage != null) leftSlotImage.sprite = availableSprites[leftIndex];
            if (rightSlotImage != null) rightSlotImage.sprite = availableSprites[rightIndex];
        }
    }

    private void CheckSolution()
    {
        if (leftIndex == winningLeftIndex && rightIndex == winningRightIndex)
        {
            Debug.Log("Terminal Hacked Successfully!");
        
            if (targetTurret != null)
            {
                targetTurret.DisableTurret();
            }
        }
        else
        {
            Debug.Log("Incorrect combination!");
            
            // Play the error sound directly from the physical terminal object in the world
            if (activeTerminal != null)
            {
                activeTerminal.PlayIncorrectSound();
            }
        }

        CloseTerminal();
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