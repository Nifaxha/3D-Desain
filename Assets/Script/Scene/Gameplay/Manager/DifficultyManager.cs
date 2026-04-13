using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    [Header("References")]
    public FloorManager floorManager;
    public FoodSpawner foodSpawner;

    [Header("Difficulty Timer")]
    public float timer = 0f;

    [Header("Level 1")]
    public float level2Time = 30f;

    [Header("Level 2")]
    public float level3Time = 60f;

    private bool level2Applied = false;
    private bool level3Applied = false;

    private void Update()
    {
        timer += Time.deltaTime;

        if (!level2Applied && timer >= level2Time)
        {
            ApplyLevel2();
            level2Applied = true;
        }

        if (!level3Applied && timer >= level3Time)
        {
            ApplyLevel3();
            level3Applied = true;
        }
    }

    private void ApplyLevel2()
    {
        Debug.Log("Difficulty Level 2");

        if (floorManager != null)
        {
            floorManager.minInterval = 1.5f;
            floorManager.maxInterval = 3f;
            floorManager.minFloorsToDrop = 1;
            floorManager.maxFloorsToDrop = 2;
        }

        if (foodSpawner != null)
        {
            foodSpawner.minSpawnTime = 0.8f;
            foodSpawner.maxSpawnTime = 1.6f;
        }
    }

    private void ApplyLevel3()
    {
        Debug.Log("Difficulty Level 3");

        if (floorManager != null)
        {
            floorManager.minInterval = 1f;
            floorManager.maxInterval = 2f;
            floorManager.minFloorsToDrop = 2;
            floorManager.maxFloorsToDrop = 3;
        }

        if (foodSpawner != null)
        {
            foodSpawner.minSpawnTime = 0.5f;
            foodSpawner.maxSpawnTime = 1.2f;
        }
    }
}