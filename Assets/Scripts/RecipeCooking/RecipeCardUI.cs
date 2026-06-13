using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecipeCardUI : MonoBehaviour
{
    [SerializeField] private Button recipeCookButton;
    [SerializeField] private TMP_Text recipeNameText;
    [SerializeField] private Image icon;
    [SerializeField] private Transform ingredientsContainer;
    [SerializeField] private GameObject ingredientCardPrefab;

    private Recipe currentRecipe;
    private CookingStation cookingStation;

    private void Start()
    {
        if (recipeCookButton != null)
            recipeCookButton.onClick.AddListener(OnCookButtonClicked);
    }

    public void Initialize(CookingStation station)
    {
        cookingStation = station;
    }

    private void OnCookButtonClicked()
    {
        cookingStation.Cook(currentRecipe);
    }

    public void SetRecipe(Recipe recipe)
    {
        if (recipe == null)
        {
            Debug.LogWarning("[RecipeCardUI] Cannot set null recipe.");
            return;
        }

        currentRecipe = recipe;

        if (recipeNameText != null)
            recipeNameText.text = recipe.displayName;

        if (icon != null)
            icon.sprite = recipe.icon;

        SetIngredients();
    }

    public void SetIngredients()
    {
        if (currentRecipe == null)
        {
            Debug.LogWarning("[RecipeCardUI] Current recipe is null.");
            return;
        }

        if (ingredientsContainer == null)
        {
            Debug.LogWarning("[RecipeCardUI] Ingredients container is not assigned.");
            return;
        }

        if (ingredientCardPrefab == null)
        {
            Debug.LogWarning("[RecipeCardUI] Ingredient card prefab is not assigned.");
            return;
        }

        foreach (Transform child in ingredientsContainer)
        {
            Destroy(child.gameObject);
        }

        if (currentRecipe.ingredients == null)
        {
            Debug.LogWarning("[RecipeCardUI] Recipe has no ingredients list: " + currentRecipe.name);
            return;
        }

        foreach (IngredientAmount ingredient in currentRecipe.ingredients)
        {
            if (ingredient == null || ingredient.item == null)
                continue;

            GameObject ingredientCardObj = Instantiate(ingredientCardPrefab, ingredientsContainer);
            ingredientCardObj.SetActive(true);

            IngredientCardUI ingredientCardUI = ingredientCardObj.GetComponent<IngredientCardUI>();

            if (ingredientCardUI == null)
            {
                Debug.LogWarning("[RecipeCardUI] Ingredient card prefab has no IngredientCardUI component.");
                continue;
            }

            ingredientCardUI.SetIngredient(ingredient);
        }
    }

    public void SetCookButtonInteractable(bool enabled)
    {
        if (recipeCookButton != null)
        {
            recipeCookButton.interactable = enabled;
            recipeCookButton.enabled = enabled;
        }
    }

    public Recipe GetCurrentRecipe()
    {
        return currentRecipe;
    }
}
