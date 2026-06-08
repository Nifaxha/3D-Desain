using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance; // Singleton agar mudah dipanggil

    [Header("UI References")]
    public TMP_Text scoreText;
    public TMP_Text livesText;
    public GameObject gameOverPanel;

    [Header("Ability Notification")]
    public TMP_Text abilityNotificationText; // Masukkan UI Text di sini
    public float notificationDuration = 2f; // Berapa lama teks muncul

    [Header("References")]
    public PlayerCollector playerCollector;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        UpdateUI();
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (abilityNotificationText != null) abilityNotificationText.gameObject.SetActive(false);
    }

    private void Update()
    {
        UpdateUI();
        UpdateGameOverUI();
    }

    public void UpdateUI()
    {
        if (scoreText != null && ScoreManager.Instance != null)
        {
            scoreText.text = " " + ScoreManager.Instance.score;
        }
        if (livesText != null && playerCollector != null)
        {
            livesText.text = " " + playerCollector.lives;
        }
    }

    private void UpdateGameOverUI()
    {
        if (gameOverPanel == null || playerCollector == null) return;

        bool isGameOver = playerCollector.isDead;
        gameOverPanel.SetActive(isGameOver);

        if (isGameOver)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    // --- FUNGSI UNTUK MEMUNCULKAN NOTIFIKASI ---
    public void ShowAbilityNotification(string message)
    {
        if (abilityNotificationText != null)
        {
            StopAllCoroutines(); // Hentikan animasi sebelumnya jika ada
            StartCoroutine(ShowNotificationRoutine(message));
        }
    }

    private IEnumerator ShowNotificationRoutine(string message)
    {
        abilityNotificationText.text = message;
        abilityNotificationText.gameObject.SetActive(true);
        yield return new WaitForSeconds(notificationDuration);
        abilityNotificationText.gameObject.SetActive(false);
    }
    // ------------------------------------------

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}