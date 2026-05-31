using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class IngredientAmount
{
    public ItemData item;
    public int amount;
}

[CreateAssetMenu(menuName = "Cookbook/Recipe")]
public class Recipe : ScriptableObject
{
    public string id;
    public string displayName;
    public List<IngredientAmount> ingredients;
    public ItemData result;
    public float cookTime;
    public Sprite icon;
}
