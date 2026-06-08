using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NPCRequestQueueManager : MonoBehaviour
{
    [System.Serializable]
    public class RequestPanelUI
    {
        public GameObject root;
        public TMP_Text recipeNameText;
        public TMP_Text timerText;
        public TMP_Text rewardText;
        public Image recipeImage;

        [Header("Red Timer Overlay")]
        public Image timerFillImage;

        public GameObject completedMark;
    }

    private class RuntimeRequest
    {
        public NpcFoodRecipe recipe;
        public List<OrderFoodData> remainingFoods = new List<OrderFoodData>();
        public float timeRemaining;
        public float totalDuration;
    }

    [Header("Recipe Pool")]
    public List<NpcFoodRecipe> availableRecipes = new List<NpcFoodRecipe>();

    [Header("Queue Settings")]
    [Tooltip("Slot 0 = kanan / oldest / aktif, Slot 1 = kiri / next")]
    public RequestPanelUI[] requestSlots = new RequestPanelUI[2];

    [Header("Spawn Settings")]
    public bool spawnFirstRequestOnStart = true;
    public bool keepQueueFilled = true;
    public int maxQueueCount = 2;

    [Header("Request Timer")]
    public float requestDuration = 20f;
    public bool removeExpiredRequest = true;
    public bool shiftQueueWhenExpired = true;
    public int failPenaltyScore = 0;

    [Header("Feedback")]
    public TMP_Text resultText;

    private readonly List<RuntimeRequest> runtimeQueue = new List<RuntimeRequest>();

    public float CurrentRequestDuration => requestDuration;

    private void Start()
    {
        runtimeQueue.Clear();

        if (spawnFirstRequestOnStart)
        {
            FillQueueToMax();
        }

        RefreshUI();
    }

    private void Update()
    {
        UpdateRequestTimer();
    }

    public void SetRequestDuration(float newDuration)
    {
        requestDuration = Mathf.Max(1f, newDuration);
    }

    public bool TryConsumeFood(OrderFoodData pickedFood)
    {
        if (pickedFood == null)
            return false;

        if (runtimeQueue.Count == 0)
            return false;

        RuntimeRequest activeRequest = runtimeQueue[0];
        if (activeRequest == null || activeRequest.recipe == null)
            return false;

        int foundIndex = activeRequest.remainingFoods.FindIndex(f => f == pickedFood);
        if (foundIndex < 0)
        {
            if (resultText != null)
                resultText.text = pickedFood.displayName + " bukan kebutuhan request aktif.";

            return false;
        }

        activeRequest.remainingFoods.RemoveAt(foundIndex);

        if (activeRequest.remainingFoods.Count == 0)
        {
            CompleteActiveRequest();
        }
        else
        {
            if (resultText != null)
                resultText.text = "Progress request: " + activeRequest.recipe.recipeName;
        }

        RefreshUI();
        return true;
    }

    public void SpawnNextRequest()
    {
        if (runtimeQueue.Count >= maxQueueCount)
            return;

        RuntimeRequest request = CreateRandomRequest();
        if (request == null)
            return;

        runtimeQueue.Add(request);
        RefreshUI();
    }

    public void FillQueueToMax()
    {
        while (keepQueueFilled && runtimeQueue.Count < maxQueueCount)
        {
            RuntimeRequest request = CreateRandomRequest();
            if (request == null)
                break;

            runtimeQueue.Add(request);
        }

        RefreshUI();
    }

    private RuntimeRequest CreateRandomRequest()
    {
        List<NpcFoodRecipe> validRecipes = new List<NpcFoodRecipe>();

        for (int i = 0; i < availableRecipes.Count; i++)
        {
            NpcFoodRecipe recipe = availableRecipes[i];
            if (!IsRecipeValid(recipe))
                continue;

            validRecipes.Add(recipe);
        }

        if (validRecipes.Count == 0)
        {
            Debug.LogWarning("NPCRequestQueueManager: tidak ada recipe valid.");
            return null;
        }

        NpcFoodRecipe selected = validRecipes[Random.Range(0, validRecipes.Count)];

        RuntimeRequest request = new RuntimeRequest
        {
            recipe = selected,
            timeRemaining = requestDuration,
            totalDuration = requestDuration
        };

        for (int i = 0; i < selected.requiredFoods.Count; i++)
        {
            if (selected.requiredFoods[i] != null)
            {
                request.remainingFoods.Add(selected.requiredFoods[i]);
            }
        }

        return request;
    }

    private bool IsRecipeValid(NpcFoodRecipe recipe)
    {
        if (recipe == null)
            return false;

        if (recipe.requiredFoods == null)
            return false;

        if (recipe.requiredFoods.Count == 0 || recipe.requiredFoods.Count > 3)
            return false;

        for (int i = 0; i < recipe.requiredFoods.Count; i++)
        {
            if (recipe.requiredFoods[i] == null)
                return false;
        }

        return true;
    }

    private void CompleteActiveRequest()
    {
        if (runtimeQueue.Count == 0)
            return;

        RuntimeRequest activeRequest = runtimeQueue[0];
        if (activeRequest == null || activeRequest.recipe == null)
            return;

        int reward = activeRequest.recipe.rewardPoints;

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(reward);
        }

        if (resultText != null)
        {
            resultText.text = "Request selesai! +" + reward + " point";
        }

        runtimeQueue.RemoveAt(0);

        if (keepQueueFilled)
        {
            FillQueueToMax();
        }

        RefreshUI();
    }

    private void FailActiveRequest()
    {
        if (runtimeQueue.Count == 0)
            return;

        RuntimeRequest activeRequest = runtimeQueue[0];

        if (failPenaltyScore > 0 && ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(-failPenaltyScore);
        }

        if (resultText != null && activeRequest != null && activeRequest.recipe != null)
        {
            resultText.text = "Request gagal: " + activeRequest.recipe.recipeName;
        }

        if (removeExpiredRequest)
        {
            runtimeQueue.RemoveAt(0);

            if (keepQueueFilled && shiftQueueWhenExpired)
            {
                FillQueueToMax();
            }
        }

        RefreshUI();
    }

    private void UpdateRequestTimer()
    {
        if (runtimeQueue.Count == 0)
            return;

        RuntimeRequest activeRequest = runtimeQueue[0];
        if (activeRequest == null)
            return;

        activeRequest.timeRemaining -= Time.deltaTime;

        if (activeRequest.timeRemaining <= 0f)
        {
            FailActiveRequest();
        }
        else
        {
            RefreshUI();
        }
    }

    private void RefreshUI()
    {
        if (requestSlots == null || requestSlots.Length == 0)
            return;

        for (int i = 0; i < requestSlots.Length; i++)
        {
            bool hasRequest = i < runtimeQueue.Count;
            ApplyPanel(requestSlots[i], hasRequest ? runtimeQueue[i] : null, i == 0);
        }
    }

    private void ApplyPanel(RequestPanelUI panel, RuntimeRequest request, bool isActiveRightSlot)
    {
        if (panel == null)
            return;

        bool hasData = request != null && request.recipe != null;

        if (panel.root != null)
            panel.root.SetActive(hasData);

        if (!hasData)
        {
            ResetPanelVisual(panel);
            return;
        }

        if (panel.recipeNameText != null)
            panel.recipeNameText.text = request.recipe.recipeName;

        if (panel.rewardText != null)
            panel.rewardText.text = "+" + request.recipe.rewardPoints;

        if (panel.timerText != null)
        {
            if (isActiveRightSlot)
                panel.timerText.text = Mathf.CeilToInt(request.timeRemaining).ToString() + "s";
            else
                panel.timerText.text = "NEXT";
        }

        if (panel.recipeImage != null)
        {
            panel.recipeImage.enabled = request.recipe.recipePreviewImage != null;
            panel.recipeImage.sprite = request.recipe.recipePreviewImage;

            Color imageColor = panel.recipeImage.color;
            imageColor.a = 1f;
            panel.recipeImage.color = imageColor;
        }

        if (panel.timerFillImage != null)
        {
            if (isActiveRightSlot)
            {
                float progress = 0f;

                if (request.totalDuration > 0f)
                {
                    progress = 1f - (request.timeRemaining / request.totalDuration);
                }

                progress = Mathf.Clamp01(progress);

                panel.timerFillImage.enabled = true;
                panel.timerFillImage.fillAmount = progress;

                Color fillColor = panel.timerFillImage.color;
                fillColor.a = 0.45f;
                panel.timerFillImage.color = fillColor;
            }
            else
            {
                panel.timerFillImage.enabled = true;
                panel.timerFillImage.fillAmount = 0f;

                Color fillColor = panel.timerFillImage.color;
                fillColor.a = 0.25f;
                panel.timerFillImage.color = fillColor;
            }
        }

        if (panel.completedMark != null)
            panel.completedMark.SetActive(false);
    }

    private void ResetPanelVisual(RequestPanelUI panel)
    {
        if (panel.recipeNameText != null)
            panel.recipeNameText.text = string.Empty;

        if (panel.timerText != null)
            panel.timerText.text = string.Empty;

        if (panel.rewardText != null)
            panel.rewardText.text = string.Empty;

        if (panel.recipeImage != null)
        {
            panel.recipeImage.sprite = null;
            panel.recipeImage.enabled = false;
        }

        if (panel.timerFillImage != null)
        {
            panel.timerFillImage.fillAmount = 0f;
            panel.timerFillImage.enabled = false;
        }

        if (panel.completedMark != null)
            panel.completedMark.SetActive(false);
    }
}