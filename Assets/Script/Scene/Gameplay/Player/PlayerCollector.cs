using UnityEngine;

public class PlayerCollector : MonoBehaviour
{
    [Header("Player Stats")]
    public int lives = 3;
    public bool isDead = false;

    [Header("Damage Cooldown")]
    public float damageCooldown = 0.5f;

    [Header("UI References")]
    public GameObject gameOverPanel;

    // --- TAMBAHAN UNTUK ABILITY ---
    [Header("Ability System")]
    public bool isImmortal = false;
    private float abilityTimer = 0f;
    private PlayerMovement3D playerMovement;
    // ------------------------------

    private float lastDamageTime = -999f;

    private void Start()
    {
        // Mengambil referensi movement untuk mengatur Double Jump & Balloon
        playerMovement = GetComponent<PlayerMovement3D>();
    }

    private void Update()
    {
        // Menghitung mundur durasi kemampuan
        if (abilityTimer > 0)
        {
            abilityTimer -= Time.deltaTime;
            if (abilityTimer <= 0)
            {
                DeactivateAllAbilities();
            }
        }
    }

    // Fungsi untuk dipanggil oleh Mystery Box
    public void ActivateRandomAbility(float duration)
    {
        DeactivateAllAbilities();
        abilityTimer = duration;

        int rand = Random.Range(0, 3);
        string abilityName = ""; // Simpan nama kemampuan untuk UI

        if (rand == 0 && playerMovement != null)
        {
            playerMovement.canDoubleJump = true;
            abilityName = "DOUBLE JUMP ACTIVE!";
        }
        else if (rand == 1 && playerMovement != null)
        {
            playerMovement.isBalloon = true;
            abilityName = "BALLOON MODE ACTIVE!";
        }
        else if (rand == 2)
        {
            isImmortal = true;
            abilityName = "IMMORTAL ACTIVE!";
        }

        // Panggil UI Manager untuk memunculkan teks
        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.ShowAbilityNotification(abilityName);
        }

        Debug.Log("ABILITY AKTIF: " + abilityName);
    }

    private void DeactivateAllAbilities()
    {
        isImmortal = false;
        if (playerMovement != null)
        {
            playerMovement.canDoubleJump = false;
            playerMovement.isBalloon = false;
        }
        Debug.Log("Kemampuan habis, kembali normal.");
    }

    public void CollectNormalFood(FoodItem item, int scoreToAdd = 1)
    {
        if (item == null || isDead) return;

        if (ScoreManager.Instance != null) ScoreManager.Instance.AddScore(scoreToAdd);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayCorrectFood(item.transform.position);

        Debug.Log("Ambil makanan biasa: +" + scoreToAdd + " poin");
    }

    public void CollectCorrectRequestFood(FoodItem item)
    {
        if (item == null || isDead) return;

        if (AudioManager.Instance != null) AudioManager.Instance.PlayCorrectFood(item.transform.position);
        Debug.Log("Ambil makanan request yang cocok");
    }

    public void CollectWrongFood(FoodItem item, int damage = 1)
    {
        if (item == null || isDead) return;

        // Jika Immortal aktif, lewati damage (kebal makan tulang)
        if (isImmortal)
        {
            Debug.Log("IMMORTAL AKTIF: Kebal dari makanan salah (Tulang)!");
            return; 
        }

        if (AudioManager.Instance != null) AudioManager.Instance.PlayWrongFood(item.transform.position);
        
        TakeDamage(damage);
        Debug.Log("Ambil makanan salah: -" + damage + " nyawa");
    }

    public void LoseLifeFromFall()
    {
        if (isDead) return;

        lives--;
        Debug.Log("Player jatuh! Lives tersisa: " + lives);

        if (lives <= 0) Die();
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        // Tambahan penjagaan: Kebal dari segala damage jika Immortal
        if (isImmortal) return;

        if (Time.time < lastDamageTime + damageCooldown) return;

        lastDamageTime = Time.time;
        lives -= amount;

        Debug.Log("Player terkena damage! Lives tersisa: " + lives);

        if (lives <= 0) Die();
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log("GAME OVER");

        if (playerMovement != null) playerMovement.enabled = false;

        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        Time.timeScale = 0f;
        AudioListener.pause = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}