using UnityEngine;

public class ExitTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Check if the player walked into the trigger volume
        if (other.CompareTag("Player"))
        {
            Debug.Log("[WIN] Player reached the exit! Triggering Game Win.");
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.SetState(GameState.GameWin);
            }
        }
    }
}