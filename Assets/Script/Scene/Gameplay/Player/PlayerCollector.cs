using UnityEngine;

public class PlayerCollector : MonoBehaviour
{
    [Header("Player Stats")]
    public int lives = 3;
    public bool isDead = false;

    [Header("Damage Cooldown")]
    public float damageCooldown = 0.5f;

    private float lastDamageTime = -999f;

    public void Collect(FoodItem item)
    {
        if (item == null || isDead) return;

        switch (item.foodType)
        {
            case FoodItem.FoodType.Normal:
                if (ScoreManager.Instance != null)
                {
                    ScoreManager.Instance.AddScore(item.scoreValue);
                }
                break;

            case FoodItem.FoodType.Golden:
                if (ScoreManager.Instance != null)
                {
                    ScoreManager.Instance.AddScore(item.scoreValue * 3);
                }
                break;

            case FoodItem.FoodType.Bomb:
                TakeDamage(1);
                break;

            case FoodItem.FoodType.SpeedBoost:
                break;

            case FoodItem.FoodType.Shield:
                break;
        }
    }

    public void LoseLifeFromFall()
    {
        if (isDead) return;

        lives--;
        Debug.Log("Player jatuh! Lives tersisa: " + lives);

        if (lives <= 0)
        {
            Die();
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        if (Time.time < lastDamageTime + damageCooldown)
            return;

        lastDamageTime = Time.time;
        lives -= amount;

        Debug.Log("Player terkena damage! Lives tersisa: " + lives);

        if (lives <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log("GAME OVER");

        PlayerMovement3D movement = GetComponent<PlayerMovement3D>();
        if (movement != null)
        {
            movement.enabled = false;
        }
    }
}