using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NpcFoodRecipe", menuName = "Food Drop 3D/NPC Food Recipe")]
public class NpcFoodRecipe : ScriptableObject
{
    [Header("Recipe Info")]
    public string recipeName = "Burger Combo";

    [Tooltip("Isi makanan yang dibutuhkan recipe ini. Maksimal 3.")]
    public List<OrderFoodData> requiredFoods = new List<OrderFoodData>();

    [Header("Recipe UI")]
    [Tooltip("1 gambar preview recipe lengkap, misalnya burger + fries dalam 1 gambar.")]
    public Sprite recipePreviewImage;

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