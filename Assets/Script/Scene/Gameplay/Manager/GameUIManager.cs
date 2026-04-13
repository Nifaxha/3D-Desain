using TMPro;
using UnityEngine;

public class GameUIManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text scoreText;
    public TMP_Text livesText;
    public GameObject gameOverPanel;

    [Header("References")]
    public PlayerCollector playerCollector;

    private void Start()
    {
        UpdateUI();
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    private void Update()
    {
        UpdateUI();
        UpdateGameOverUI();
    }

    public void UpdateUI()
    {
        if (scoreText != null)
        {
            int currentScore = 0;

            if (ScoreManager.Instance != null)
            {
                currentScore = ScoreManager.Instance.score;
            }

            scoreText.text = "Score: " + currentScore;
        }

        if (livesText != null && playerCollector != null)
        {
            livesText.text = "Lives: " + playerCollector.lives;
        }
    }

    private void UpdateGameOverUI()
    {
        if (gameOverPanel == null || playerCollector == null)
            return;

        gameOverPanel.SetActive(playerCollector.isDead);
    }
}