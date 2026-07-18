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

    private void OnEnable()
    {
        if (controls != null)
        {
            controls.Player.Pause.performed += OnPauseAction;
            controls.Enable();
        }
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
        mainMenuPanel = GameObject.Find("MainMenuPanel");
        gameplayHUDPanel = GameObject.Find("GameplayHUDPanel");
        pauseMenuPanel = GameObject.Find("PauseMenuPanel");
        winMenuPanel = GameObject.Find("winMenuPanel");
        gameOverPanel = GameObject.Find("gameOverPanel");

        if (scene.name == "MainMenuScene") 
            SetState(GameState.MainMenu);
        else 
            SetState(GameState.Playing);
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
            case GameState.GameWin:
                Time.timeScale = 0f;
                SetActivePanel(winMenuPanel);
                break;
            case GameState.GameOver:
                Time.timeScale = 0f;
                SetActivePanel(gameOverPanel);
                break;
        }
    }

    private void SetActivePanel(GameObject panel)
    {
        if (mainMenuPanel) mainMenuPanel.SetActive(panel == mainMenuPanel);
        if (gameplayHUDPanel) gameplayHUDPanel.SetActive(panel == gameplayHUDPanel);
        if (pauseMenuPanel) pauseMenuPanel.SetActive(panel == pauseMenuPanel);
        if (winMenuPanel) winMenuPanel.SetActive(panel == winMenuPanel);
        if (gameOverPanel) gameOverPanel.SetActive(panel == gameOverPanel);
    }

    private void LockCursorDelayed()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}