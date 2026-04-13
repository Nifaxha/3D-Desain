using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GamePaceTimer : MonoBehaviour
{
    [System.Serializable]
    public class PaceStage
    {
        [Header("Trigger Time")]
        [Tooltip("Menit ke berapa stage ini mulai aktif.")]
        public float startMinute = 0f;

        [Header("Food Spawn")]
        public float foodMinSpawnTime = 1f;
        public float foodMaxSpawnTime = 2.5f;

        [Header("Food Fall Speed")]
        public float foodFallSpeedMultiplier = 1f;

        [Header("Floor Fall")]
        public float floorMinInterval = 2f;
        public float floorMaxInterval = 5f;

        [Header("Landslide")]
        public float landslideMinInterval = 6f;
        public float landslideMaxInterval = 10f;

        [Header("NPC Request")]
        public float requestDuration = 20f;
    }

    [Header("References")]
    public FoodSpawner foodSpawner;
    public FloorManager floorManager;
    public LandslideTrapManager landslideTrapManager;
    public NPCRequestQueueManager npcRequestQueueManager;

    [Header("Timer UI")]
    public TMP_Text timerText;
    public string timerPrefix = "Time: ";

    [Header("Stages")]
    public List<PaceStage> stages = new List<PaceStage>();

    [Header("Debug")]
    public bool applyFirstStageOnStart = true;

    private float elapsedTime;
    private int currentStageIndex = -1;

    public float ElapsedTime => elapsedTime;
    public float ElapsedMinutes => elapsedTime / 60f;

    private void Start()
    {
        SortStages();

        if (applyFirstStageOnStart && stages.Count > 0)
        {
            ApplyStage(0);
        }

        UpdateTimerUI();
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;

        CheckStageUpdate();
        UpdateTimerUI();
    }

    private void SortStages()
    {
        stages.Sort((a, b) => a.startMinute.CompareTo(b.startMinute));
    }

    private void CheckStageUpdate()
    {
        if (stages.Count == 0)
            return;

        float currentMinute = elapsedTime / 60f;

        int bestStage = -1;

        for (int i = 0; i < stages.Count; i++)
        {
            if (currentMinute >= stages[i].startMinute)
            {
                bestStage = i;
            }
            else
            {
                break;
            }
        }

        if (bestStage != -1 && bestStage != currentStageIndex)
        {
            ApplyStage(bestStage);
        }
    }

    private void ApplyStage(int stageIndex)
    {
        if (stageIndex < 0 || stageIndex >= stages.Count)
            return;

        PaceStage stage = stages[stageIndex];
        currentStageIndex = stageIndex;

        if (foodSpawner != null)
        {
            foodSpawner.minSpawnTime = stage.foodMinSpawnTime;
            foodSpawner.maxSpawnTime = stage.foodMaxSpawnTime;
        }

        FoodItem.GlobalFallSpeedMultiplier = stage.foodFallSpeedMultiplier;

        if (floorManager != null)
        {
            floorManager.minInterval = stage.floorMinInterval;
            floorManager.maxInterval = stage.floorMaxInterval;
        }

        if (landslideTrapManager != null)
        {
            landslideTrapManager.minInterval = stage.landslideMinInterval;
            landslideTrapManager.maxInterval = stage.landslideMaxInterval;
        }

        if (npcRequestQueueManager != null)
        {
            npcRequestQueueManager.SetRequestDuration(stage.requestDuration);
        }

        Debug.Log("GamePaceTimer apply stage: " + stageIndex + " at minute " + stage.startMinute);
    }

    private void UpdateTimerUI()
    {
        if (timerText == null)
            return;

        int totalSeconds = Mathf.FloorToInt(elapsedTime);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timerText.text = timerPrefix + minutes.ToString("00") + ":" + seconds.ToString("00");
    }
}