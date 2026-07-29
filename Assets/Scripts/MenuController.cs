using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : Singleton<MenuController>
{
    [SerializeField] private string gameSceneName = "SampleScene";

    private Button _playButton;
    
    

    public void OnEnable()
    {
        _playButton.onClick.AddListener(PlayGame);
    }

    public void PlayGame()
    {
        if (GameStateManager.Instance != null)
        {
            // Corrected: Accessing enum directly
            GameStateManager.Instance.SetState(GameState.Playing);
        }

        SceneManager.LoadScene(gameSceneName);
    }
    
    public void ResumeGame()
    {
        if (GameStateManager.Instance != null)
        {
            // Corrected: Accessing enum directly
            GameStateManager.Instance.SetState(GameState.Playing);
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quitting game...");
    }
}