using UnityEngine;

[CreateAssetMenu(fileName = "IngredientData", menuName = "Scriptable Objects/IngredientData")]
public class IngredientData : ScriptableObject
{
    public string id;
    public string displayName;
    public Sprite icon;

    [Header("Stack")]
    public bool isStackable;
    public int maxStackSize = 1;

    [Header("World")]
    public GameObject worldPrefab;
}
//add test comment 
