using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecipeCardUI : MonoBehaviour
{
    [SerializeField] private CookingStation cookingStation;
    [SerializeField] private Button recipeCookButton;
    [SerializeField] private TMP_Text recipeNameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Image icon;

    private Recipe currentRecipe;

    private void Start()
    {
        if (recipeCookButton != null)
            recipeCookButton.onClick.AddListener(OnCookButtonClicked);
    }

    private void OnCookButtonClicked()
    {
        cookingStation.Cook(currentRecipe);
    }

    public void SetRecipe(Recipe recipe)
    {
        if (recipeNameText != null)
            recipeNameText.text = recipe.displayName;

        if (descriptionText != null)
        {
            string ingredientsList = "";
            foreach (var ingredient in recipe.ingredients)
            {
                ingredientsList += $"{ingredient.amount}x {ingredient.item.displayName}\n";
            }
            descriptionText.text = $"Ingredients:\n{ingredientsList}";
        }
        if (icon != null)
            icon.sprite = recipe.icon;

        currentRecipe = recipe;
    }

    public void SetCookButtonInteractable(bool enabled)
    {
        if (recipeCookButton != null)
        {
            recipeCookButton.interactable = enabled;
            recipeCookButton.enabled = enabled;
        }
    }
}
