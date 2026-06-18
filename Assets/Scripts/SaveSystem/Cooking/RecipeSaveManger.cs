using System.Collections.Generic;
using UnityEngine;

public class RecipeSaveManager : MonoBehaviour
{
    public static RecipeSaveManager Instance { get; private set; }

    [Header("Recipes Database")]
    [SerializeField] private RecipeDatabase recipeDatabase;

    [SerializeField] private RecipeProgressManager recipeProgressManager;
    [SerializeField] private CookbookUI cookbookUI;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if(recipeDatabase == null)
        {
            Debug.LogError("[RECIPE SAVE] Recipe database reference is missing in the inspector.");
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public RecipeSaveData CaptureSaveData()
    {
        Recipe currentRecipe = recipeProgressManager.CurrentActiveRecipe;
        
        RecipeSaveData saveData = new RecipeSaveData();
        if (currentRecipe == null)
        {
            Debug.LogWarning("[RECIPE SAVE] current recipe is NULL.");
            return saveData;
        }
        if (currentRecipe != null)
        {
            saveData.recipeId = currentRecipe.id;
        }
        return saveData;
    }

    public void RestoreSaveData(RecipeSaveData currentRecipe)
    {
        if (currentRecipe == null)
        {
            Debug.LogWarning("[RECIPE SAVE] current recipe is NULL. Cannot restore save data.");
            return;
        }
        Recipe recipe = recipeDatabase.GetRecipeById(currentRecipe.recipeId);
        if(recipe == null)
        {
            Debug.LogWarning($"[RECIPE SAVE] No recipe found with ID: {currentRecipe.recipeId}. Cannot restore save data.");
            return;
        }

        ChangeCurrentRecipeInGame(recipe);
    }

    private void ChangeCurrentRecipeInGame(Recipe recipe)
    {
        recipeProgressManager.SetCurrentRecipe(recipe);

        if (cookbookUI != null)
        {
            cookbookUI.RefreshRecipeCards();
        }
        else
        {
            Debug.LogWarning("[RECIPE SAVE] CookbookUI reference is missing. Cannot refresh recipe cards.");
        }

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.SetCurrentActiveRecipe(recipe);
        }
        else
        {
            Debug.LogWarning("[RECIPE SAVE] QuestManager instance is missing. Cannot set current active recipe for quests.");
        }
    }
}