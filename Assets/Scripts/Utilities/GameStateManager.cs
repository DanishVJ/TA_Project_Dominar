using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameStateManager : Singleton<GameStateManager>
{
    private PlayerControls _controls; 

    [Header("Current State")]
    [SerializeField] private GameState currentState;
    public GameState CurrentState => currentState;

    [SerializeField] private Canvas _canvas;

    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject gameplayHUDPanel;
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject winMenuPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject terminalHackingPanel;

    private GameObject _currentActivePanel; // Tracks currently visible UI panel

    private void Awake()
    {
        base.Awake();
    }
    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("OnSceneLoaded");
        mainMenuPanel = Resources.Load<GameObject>("Prefabs/MainMenuPanel");
        gameplayHUDPanel = Resources.Load<GameObject>("Prefabs/GameplayHUDPanel");
        pauseMenuPanel = Resources.Load<GameObject>("Prefabs/PauseMenuPanel");
        winMenuPanel = Resources.Load<GameObject>("Prefabs/WinMenuPanel");
        gameOverPanel = Resources.Load<GameObject>("Prefabs/GameOverPanel");
        terminalHackingPanel = Resources.Load<GameObject>("Prefabs/TerminalHackingPanel");
        
        _canvas = FindFirstObjectByType<Canvas>();
        
        mainMenuPanel = Instantiate(mainMenuPanel, _canvas.transform);
        gameplayHUDPanel = Instantiate(gameplayHUDPanel, _canvas.transform);
        pauseMenuPanel = Instantiate(pauseMenuPanel, _canvas.transform);
        winMenuPanel = Instantiate(winMenuPanel, _canvas.transform);
        gameOverPanel = Instantiate(gameOverPanel, _canvas.transform);
        terminalHackingPanel = Instantiate(terminalHackingPanel, _canvas.transform);
        
        Debug.Log("loaded");
        if (scene.name == "MainMenuScene")
        {
            SetState(GameState.MainMenu);
            Debug.Log("loaded main");
        }
        else
        {
            SetState(GameState.Playing);
            Debug.Log("loaded playing");
        }
    }

    public void TogglePause()
    {
        if (currentState == GameState.Playing)
        {
            SetState(GameState.Paused);
        }
        else if (currentState == GameState.Paused)
        {
            SetState(GameState.Playing);
        }
    }

    public void SetState(GameState newState)
    {
        currentState = newState;
        HandleStateChanges(currentState);
    }

    private void HandleStateChanges(GameState state)
    {
        CancelInvoke("LockCursorDelayed");

        switch (state)
        {
            case GameState.MainMenu:
                Time.timeScale = 0f; 
                Cursor.lockState = CursorLockMode.None; 
                Cursor.visible = true;
                SetActivePanel(mainMenuPanel);
                break;
            case GameState.Playing:
                Time.timeScale = 1f; 
                Invoke("LockCursorDelayed", 0.1f);
                SetActivePanel(gameplayHUDPanel);
                break;
            case GameState.Paused:
                Time.timeScale = 0f; 
                Cursor.lockState = CursorLockMode.None; 
                Cursor.visible = true;
                SetActivePanel(pauseMenuPanel);
                break;
            case GameState.TerminalHacking:
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None; 
                Cursor.visible = true;
                SetActivePanel(terminalHackingPanel);
                break;
            case GameState.GameWin:
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None; 
                Cursor.visible = true;
                SetActivePanel(winMenuPanel);
                break;
            case GameState.GameOver:
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None; 
                Cursor.visible = true;
                SetActivePanel(gameOverPanel);
                break;
        }
    }

    private void SetActivePanel(GameObject panel)
    {
        // Automatically hide the previously open panel
        if (_currentActivePanel != null && _currentActivePanel != panel)
        {
            _currentActivePanel.SetActive(false);
        }

        if (panel != null)
        {
            panel.SetActive(true);
            _currentActivePanel = panel; // Update tracker
        }
        else
        {
            Debug.LogWarning("Attempted to set an active panel, but the panel reference is null!");
        }
    }

    private void LockCursorDelayed()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}