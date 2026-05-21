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

    private void Awake()
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

            if (i == 0)
            {
                //set the first recipe for the used template card
                newCard.SetCookButtonInteractable(true);
                recipeProgressManager.SetFirstRecipe(recipe);
            }
            else
            {
                //set by default
                newCard.SetCookButtonInteractable(false);
            }

            RegisterRecipeCard(recipe, newCard);
        }
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