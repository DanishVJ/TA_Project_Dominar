using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : Singleton<InputManager>
{
    private PlayerControls _controls;
    private PlayerController _playerController;
    
    public static event Action<Vector2> OnTerminalNavigate;
    public static event Action OnTerminalSubmit;

    protected override void Awake()
    {
        base.Awake();
        _controls = new PlayerControls();
        _controls.Enable();
        _playerController = FindFirstObjectByType<PlayerController>();
    }

    private void OnEnable()
    {
        _controls.Player.Move.performed += OnMove;
        _controls.Player.Move.canceled += OnMove;

        _controls.Player.Jump.performed += OnJump;

        _controls.Player.Pause.performed += OnPauseAction;

        _controls.Player.Interact.performed += OnInteractAction;
        
        _controls.Player.TerminalNavigate.performed += OnTerminalNavigateAction;
        _controls.Player.TerminalNavigate.canceled += OnTerminalNavigateAction;

        _controls.Player.TerminalSubmit.performed += OnTerminalSubmitAction;
    }

    private void OnDisable()
    {
        _controls.Player.Move.performed -= OnMove;
        _controls.Player.Move.canceled -= OnMove;

        _controls.Player.Jump.performed -= OnJump;

        _controls.Player.Pause.performed -= OnPauseAction;

        _controls.Player.Interact.performed -= OnInteractAction;
        
        _controls.Player.TerminalNavigate.performed -= OnTerminalNavigateAction;
        _controls.Player.TerminalNavigate.canceled -= OnTerminalNavigateAction;

        _controls.Player.TerminalSubmit.performed -= OnTerminalSubmitAction;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _playerController.moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        _playerController.OnJump();
    }

    public void OnPauseAction(InputAction.CallbackContext context)
    {
        GameStateManager.Instance.TogglePause();
    }

    public void OnInteractAction(InputAction.CallbackContext context)
    {
        _playerController.OnInteract();
    }
    
    public void OnTerminalNavigateAction(InputAction.CallbackContext context)
    {
        OnTerminalNavigate?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnTerminalSubmitAction(InputAction.CallbackContext context)
    {
        OnTerminalSubmit?.Invoke();
    }
    
}