using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class VictoryController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private Button restartButton;

    private void Start()
    {
        // Keep victory panel hidden at start
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }

        // Wire up the Play Again button to reload the game
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // When the player steps into the exit zone
        if (collision.GetComponent<PlayerController>() != null)
        {
            Debug.Log("[VictoryController] Player reached the exit! VICTORY!");

            if (victoryPanel != null)
            {
                victoryPanel.SetActive(true);
            }

            // Freeze the player in celebration
            PlayerController.CanMove = false;
        }
    }

    public void RestartGame()
    {
        // Unfreeze and reload the current scene
        PlayerController.CanMove = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}