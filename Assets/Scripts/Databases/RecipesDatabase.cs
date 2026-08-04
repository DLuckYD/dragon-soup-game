using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Databases/Recipe Database")]
public class RecipeDatabase : ScriptableObject
{
    [Header("Recipes")]
    [SerializeField] private List<Recipe> recipes = new List<Recipe>();

#if UNITY_EDITOR
    [Header("Editor Auto Fill")]
    [SerializeField] private string itemDataFolderPath = "Assets";

    [ContextMenu("Rebuild Item Database")]
    private void RebuildDatabase()
    {
        recipes.Clear();

        string[] guids = AssetDatabase.FindAssets("t:Recipe", new[] { itemDataFolderPath });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Recipe item = AssetDatabase.LoadAssetAtPath<Recipe>(path);

            if (item != null && !recipes.Contains(item))
            {
                recipes.Add(item);
            }
        }

        recipes.Sort((a, b) => a.id.CompareTo(b.id));

        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();

        Debug.Log("Recipe Database rebuilt. Recipes found: " + recipes.Count);
    }
#endif

    public Recipe GetRecipeById(string id)
    {
        foreach (Recipe item in recipes)
        {
            if (item.id.Equals(id))
                return item;
        }

        Debug.LogWarning("Recipe not found with id: " + id);
        return null;
    }

    public List<Recipe> GetAllRecipes()
    {
        return recipes;
    }
}