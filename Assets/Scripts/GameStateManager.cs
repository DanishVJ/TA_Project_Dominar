using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    private PlayerControls controls; 

    [Header("Current State")]
    [SerializeField] private GameState currentState;
    public GameState CurrentState => currentState;

    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject gameplayHUDPanel;
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject winMenuPanel;
    [SerializeField] private GameObject gameOverPanel;

    private void Awake()
    {
        if (Instance == null) 
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else 
        { 
            Destroy(gameObject); 
            return; 
        }

        controls = new PlayerControls();
    }

    private void Start() 
    {
        // Set initial state based on starting scene
        if (SceneManager.GetActiveScene().name == "MainMenuScene") 
            SetState(GameState.MainMenu);
        else 
            SetState(GameState.Playing);
    }

    private void OnEnable()
    {
        if (controls != null)
        {
            controls.Player.Pause.performed += OnPauseAction;
            controls.Enable();
        }
        // Listen for scene changes to update UI references
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        if (controls != null)
        {
            controls.Player.Pause.performed -= OnPauseAction;
            controls.Disable();
        }
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Re-run handle state to refresh panel references for the new scene
        SetState(currentState);
    }

    private void OnPauseAction(InputAction.CallbackContext context) => TogglePause();

    public void TogglePause()
    {
        if (currentState == GameState.Playing) SetState(GameState.Paused);
        else if (currentState == GameState.Paused) SetState(GameState.Playing);
    }

    public void SetState(GameState newState)
    {
        currentState = newState;
        HandleStateChanges(currentState);
    }

    private void HandleStateChanges(GameState state)
    {
        // Hide all first
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (gameplayHUDPanel != null) gameplayHUDPanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (winMenuPanel != null) winMenuPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        CancelInvoke("LockCursorDelayed");

        switch (state)
        {
            case GameState.MainMenu:
                Time.timeScale = 0f; 
                Cursor.lockState = CursorLockMode.None; 
                Cursor.visible = true;
                if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
                break;

            case GameState.Playing:
                Time.timeScale = 1f; 
                Invoke("LockCursorDelayed", 0.1f);
                if (gameplayHUDPanel != null) gameplayHUDPanel.SetActive(true);
                break;

            case GameState.Paused:
                Time.timeScale = 0f; 
                Cursor.lockState = CursorLockMode.None; 
                Cursor.visible = true;
                if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
                break;

            case GameState.GameWin:
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                if (winMenuPanel != null) winMenuPanel.SetActive(true);
                break;

            case GameState.GameOver:
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                if (gameOverPanel != null) gameOverPanel.SetActive(true);
                break;
        }
    }

    private void LockCursorDelayed()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}