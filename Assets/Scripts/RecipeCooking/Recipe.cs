using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class IngredientAmount
{
    public IngredientData item;
    public int amount;
}

[CreateAssetMenu(menuName = "Cookbook/Recipe")]
public class Recipe : ScriptableObject
{
    public string id;
    public string displayName;
    public List<IngredientAmount> ingredients;
    public IngredientData result;
    public float cookTime;
    public Sprite icon;
}
