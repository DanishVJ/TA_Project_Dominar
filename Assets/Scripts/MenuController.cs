using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "SampleScene";

    public void PlayGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }
    
    public void ResumeGame()
    {
        if (GameStateManager.Instance != null)
        {
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