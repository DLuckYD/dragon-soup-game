using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DismantleRecipe", menuName = "Scriptable Objects/DismantleRecipe")]
public class DismantleRecipe : ScriptableObject
{
    public List<DismantleOutput> outputs = new List<DismantleOutput>();
}

[System.Serializable]
public class DismantleOutput
{
    public IngredientData itemData;
    public int amount = 1;
}