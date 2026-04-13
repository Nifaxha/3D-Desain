using UnityEngine;

[CreateAssetMenu(fileName = "OrderFoodData", menuName = "Food Drop 3D/Order Food Data")]
public class OrderFoodData : ScriptableObject
{
    [Header("Identity")]
    public string foodId = "burger";
    public string displayName = "Burger";

    [Header("UI")]
    public Sprite icon;
}