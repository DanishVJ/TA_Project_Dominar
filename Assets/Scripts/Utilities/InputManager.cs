using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : Singleton<InputManager>
{
    private PlayerControls _controls;
    private PlayerController _playerController;

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
    }
    
    private void OnDisable()
    {
        _controls.Player.Move.performed -= OnMove;
        _controls.Player.Move.canceled -= OnMove;
        
        _controls.Player.Jump.performed -= OnJump;
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

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
