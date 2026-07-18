using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "SampleScene";

    public void PlayGame()
    {
        // Tell the persistent manager to start the game
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SetState(GameStateManager.GameState.Playing);
        }

        // Load the game scene
        SceneManager.LoadScene(gameSceneName);
    }
    
    public void ResumeGame()
    {
        // Toggle back to playing from the pause state
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SetState(GameStateManager.GameState.Playing);
        }
    }

    public void RestartGame()
    {
        // Reloads current scene and triggers Start() in GameStateManager
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quitting game...");
    }
}