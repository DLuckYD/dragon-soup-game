using System;
using System.Collections.Generic;
using UnityEngine;

public class CookbookUI : MonoBehaviour
{
    [Header("CookBook")]
    public GameObject cookBookPanel;
    public FirstPersonCamera cameraScript;

    [Header("Recipe UI")]
    [SerializeField] private RecipeDatabase recipeDatabase;
    [SerializeField] private Transform recipeListContainer;
    [SerializeField] private RecipeCardUI recipeCard;

    [SerializeField] private RecipeProgressManager recipeProgressManager;

    private bool isCookBookOpen = false;
    private bool recipesGenerated = false;
    private Dictionary<string, RecipeCardUI> recipeCardsById = new Dictionary<string, RecipeCardUI>();

    private void Start()
    {
        isCookBookOpen = false;

        if (cookBookPanel != null)
            cookBookPanel.SetActive(false);

        if (!recipesGenerated)
        {
            GenerateRecipeCards();
            recipesGenerated = true;
        }
    }

    private void OnEnable()
    {
        RecipeProgressManager.OnCurrentRecipeChanged += HandleCurrentRecipeChanged;
        Debug.Log("[COOKBOOK UI] Subscribed to RecipeProgressManager.OnCurrentRecipeChanged event.");
    }

    private void OnDisable()
    {
        RecipeProgressManager.OnCurrentRecipeChanged -= HandleCurrentRecipeChanged;
    }

    public void OpenCookBook()
    {
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (cameraScript != null) cameraScript.canLook = false;

        isCookBookOpen = true;
        cookBookPanel.SetActive(true);
    }

    public bool IsOpened()
    {
        return isCookBookOpen;
    }

    public void CloseCookBook()
    {
        isCookBookOpen = false;
        cookBookPanel.SetActive(false);

        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        if (cameraScript != null) cameraScript.canLook = true;
    }

    void GenerateRecipeCards()
    {
        if (recipeDatabase == null || recipeCard == null || recipeListContainer == null)
        {
            Debug.LogWarning("CookbookUI: Missing references for generating recipe cards.");
            return;
        }

        List<Recipe> recipes = recipeDatabase.GetAllRecipes();
        int recipeCount = recipes.Count;

        if (recipeCount == 0)
        {
            recipeCard.gameObject.SetActive(false);
            Debug.Log("No recipes found.");
            return;
        }


        recipeCard.gameObject.SetActive(false);

        // other recipes will be generated as new cards
        for (int i = 0; i < recipeCount; i++)
        {
            Recipe recipe = recipes[i];

            if (recipe == null)
                continue;

            RecipeCardUI newCard = Instantiate(recipeCard, recipeListContainer);
            newCard.gameObject.SetActive(true);
            newCard.SetRecipe(recipe);

            Recipe activeRecipe = recipeProgressManager.CurrentActiveRecipe;
            Debug.Log($"[COOKBOOK UI] Setting up card for recipe '{recipe.id}'. Active recipe: {(activeRecipe != null ? activeRecipe.id : "NULL")}");
            if (activeRecipe == null)
            {
                newCard.SetCookButtonInteractable(i == 0);

                if (i == 0)
                    recipeProgressManager.SetCurrentRecipe(recipe);
            }
            else
            {
                newCard.SetCookButtonInteractable(recipe.id == activeRecipe.id);
            }

            RegisterRecipeCard(recipe, newCard);
        }
    }

    private void HandleCurrentRecipeChanged(Recipe recipe)
    {
        Debug.Log("[COOKBOOK UI] Current active recipe changed to: " + (recipe != null ? recipe.id : "NULL"));
        RefreshRecipeCards();
    }

    public void RefreshRecipeCards()
    {
        Debug.Log("[COOKBOOK UI] Refreshing recipe cards...");
        if (recipeProgressManager == null)
            return;

        Recipe activeRecipe = recipeProgressManager.CurrentActiveRecipe;

        foreach (var pair in recipeCardsById)
        {
            string recipeId = pair.Key;
            RecipeCardUI card = pair.Value;

            bool isActive = activeRecipe != null && recipeId == activeRecipe.id;
            card.SetCookButtonInteractable(isActive);
        }

        Debug.Log("[COOKBOOK UI] Refreshed cards. Active recipe: " +
                  (activeRecipe != null ? activeRecipe.id : "NULL"));
    }

    private void RegisterRecipeCard(Recipe recipe, RecipeCardUI newCard)
    {
        if (recipe == null || newCard == null)
            return;

        if (string.IsNullOrEmpty(recipe.id))
        {
            Debug.Log("Recipe id is null or empty");
            return;
        }

        if (!recipeCardsById.ContainsKey(recipe.id))
        {
            recipeCardsById.Add(recipe.id, newCard);
        }
    }

    internal void MarkRecipeAsInteractable(string recipeId, bool interactable)
    {
        if (string.IsNullOrEmpty(recipeId)) return;

        if (recipeCardsById.TryGetValue(recipeId, out RecipeCardUI card))
        {
            card.SetCookButtonInteractable(interactable);
        }
    }

    public Recipe GetRecipeById(string recipeId)
    {
        if (string.IsNullOrEmpty(recipeId)) return null;
        if (recipeCardsById.TryGetValue(recipeId, out RecipeCardUI card))
        {
            return card.GetCurrentRecipe();
        }
        return null;
    }
}