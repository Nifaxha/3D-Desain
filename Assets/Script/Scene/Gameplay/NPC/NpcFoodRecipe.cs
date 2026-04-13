using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NpcFoodRecipe", menuName = "Food Drop 3D/NPC Food Recipe")]
public class NpcFoodRecipe : ScriptableObject
{
    [Header("Recipe Info")]
    public string recipeName = "Burger Combo";

    [Tooltip("Isi 1 sampai 3 makanan. Sebaiknya unik, tidak duplikat.")]
    public List<OrderFoodData> requiredFoods = new List<OrderFoodData>();

    [Header("Reward")]
    public int rewardPoints = 10;

    private void OnValidate()
    {
        if (requiredFoods == null)
            requiredFoods = new List<OrderFoodData>();

        if (requiredFoods.Count > 3)
        {
            requiredFoods.RemoveRange(3, requiredFoods.Count - 3);
        }
    }
}