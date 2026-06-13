//using System;
//using System.Collections.Generic;
//using UnityEngine;

//public class CookingStation : MonoBehaviour
//{
//    public HotbarManager playerInventory;
//    public CookbookUI cookBook;
//    public RecipeProgressManager recipeProcessManager;

//    [Header("Cauldron UI")]
//    [SerializeField] private CauldronUI cauldronUI;

//    [Header("Cooking Settings")]
//    [SerializeField] private float addIngredientTime = 10f;

//    private HashSet<string> cookedRecipes = new HashSet<string>();

//    public static event Action<string> OnMissingIngredients;
//    public static event Action<string> OnSuccessfulCook;

//    private Recipe activeRecipe;
//    private int currentIngredientIndex = -1;
//    private float timer = 0f;

//    private CookingState cookingState = CookingState.None;

//    public bool isCookBookOpen => cookBook != null && cookBook.IsOpen;

//    private enum CookingState
//    {
//        None,
//        WaitingForIngredient,
//        CookingIngredient,
//        Finished
//    }

//    public bool IsWaitingForIngredient => cookingState == CookingState.WaitingForIngredient;
//    public bool IsCookingInProgress => cookingState == CookingState.WaitingForIngredient || cookingState == CookingState.CookingIngredient;

//    private void Update()
//    {
//        if (cookingState == CookingState.None || cookingState == CookingState.Finished)
//            return;

//        timer -= Time.deltaTime;

//        if (timer < 0f)
//            timer = 0f;

//        if (cauldronUI != null)
//            cauldronUI.UpdateTimer(timer);

//        if (cookingState == CookingState.WaitingForIngredient /*&& timer <= 0f*/)
//        {
//            OnMissingIngredients?.Invoke("You did not add the ingredient in time.");
//            Debug.Log("[COOKING] Failed to add ingredient in time.");

//            ResetCooking();
//            return;
//        }

//        if (cookingState == CookingState.CookingIngredient /*&& timer <= 0f*/)
//        {
//            FinishCurrentIngredient();
//        }
//    }

//    public bool CanCook(Recipe recipe, HotbarManager inventory)
//    {
//        if (recipe == null || inventory == null)
//            return false;

//        foreach (var ingredient in recipe.ingredients)
//        {
//            if (!inventory.HasItemDataAmount(ingredient.item, ingredient.amount))
//                return false;
//        }

//        return true;
//    }

//    public void Cook(Recipe recipe)
//    {
//        if (recipe == null)
//            return;

//        if (cookedRecipes.Contains(recipe.id))
//        {
//            Debug.Log("Recipe already cooked in this session");
//            return;
//        }

//        if (IsCookingInProgress)
//        {
//            Debug.Log("[COOKING] Another recipe is already cooking.");
//            return;
//        }

//        if (!CanCook(recipe, playerInventory))
//        {
//            OnMissingIngredients?.Invoke("Not enough ingredients");
//            Debug.Log("Missing ingredients");
//            return;
//        }

//        activeRecipe = recipe;
//        currentIngredientIndex = 0;

//        cookingState = CookingState.WaitingForIngredient;
//        timer = addIngredientTime;

//        if (cauldronUI != null)
//        {
//            cauldronUI.StartCookingUI(activeRecipe);
//            cauldronUI.ShowWaitingForIngredient(currentIngredientIndex, timer);
//        }

//        if (cookBook != null)
//            cookBook.CloseCookBook();

//        Debug.Log("[COOKING] Started recipe: " + recipe.displayName);
//    }

//    public void AddCurrentIngredient()
//    {
//        if (cookingState != CookingState.WaitingForIngredient)
//        {
//            Debug.Log("[COOKING] Cauldron is not waiting for an ingredient.");
//            return;
//        }

//        if (activeRecipe == null)
//            return;

//        if (currentIngredientIndex < 0 || currentIngredientIndex >= activeRecipe.ingredients.Count)
//            return;

//        IngredientAmount ingredient = activeRecipe.ingredients[currentIngredientIndex];

//        if (ingredient == null || ingredient.item == null)
//            return;

//        if (!playerInventory.HasItemDataAmount(ingredient.item, ingredient.amount))
//        {
//            OnMissingIngredients?.Invoke("Missing ingredient: " + ingredient.item.displayName);
//            Debug.Log("[COOKING] Missing current ingredient: " + ingredient.item.displayName);
//            return;
//        }

//        playerInventory.RemoveItemDataAmount(ingredient.item, ingredient.amount);

//        float cookTime = ingredient.amount * ingredient.item.cookingTime;

//        timer = cookTime;
//        cookingState = CookingState.CookingIngredient;

//        if (cauldronUI != null)
//            cauldronUI.ShowCookingIngredient(currentIngredientIndex, timer);

//        Debug.Log(
//            "[COOKING] Added ingredient: " +
//            ingredient.item.displayName +
//            " x" + ingredient.amount +
//            " | Cooking time: " + cookTime
//        );
//    }

//    private void FinishCurrentIngredient()
//    {
//        if (cauldronUI != null)
//            cauldronUI.MarkIngredientCooked(currentIngredientIndex);

//        Debug.Log("[COOKING] Ingredient cooked. Index: " + currentIngredientIndex);

//        currentIngredientIndex++;

//        if (currentIngredientIndex >= activeRecipe.ingredients.Count)
//        {
//            FinishRecipe();
//            return;
//        }

//        timer = addIngredientTime;
//        cookingState = CookingState.WaitingForIngredient;

//        if (cauldronUI != null)
//            cauldronUI.ShowWaitingForIngredient(currentIngredientIndex, timer);

//        Debug.Log("[COOKING] Waiting for next ingredient. Index: " + currentIngredientIndex);
//    }

//    private void FinishRecipe()
//    {
//        if (activeRecipe == null)
//            return;

//        playerInventory.AddCookingDishToInventory(activeRecipe.result);

//        cookedRecipes.Add(activeRecipe.id);

//        OnSuccessfulCook?.Invoke("Cooked successfully");

//        if (recipeProcessManager != null)
//            recipeProcessManager.OnRecipeCooked(activeRecipe, cookBook);

//        if (cauldronUI != null)
//            cauldronUI.ShowRecipeFinished();

//        Debug.Log("[COOKING] Recipe finished: " + activeRecipe.displayName);

//        activeRecipe = null;
//        currentIngredientIndex = -1;
//        timer = 0f;
//        cookingState = CookingState.Finished;
//    }

//    private void ResetCooking()
//    {
//        activeRecipe = null;
//        currentIngredientIndex = -1;
//        timer = 0f;
//        cookingState = CookingState.None;

//        if (cauldronUI != null)
//            cauldronUI.ResetUI();

//        Debug.Log("[COOKING] Cooking process reset.");
//    }
//}

using System.Collections.Generic;
using UnityEngine;


public class CookingStation : MonoBehaviour
{
    public HotbarManager playerInventory;
    public CookbookUI cookBook;
    public RecipeProgressManager recipeProcessManager;

    private HashSet<string> cookedRecipes = new HashSet<string>();

    public bool CanCook(Recipe recipe, HotbarManager inventory)
    {
        if (recipe == null || inventory == null)
            return false;

        foreach (var ingredient in recipe.ingredients)
        {
            if (!inventory.HasItemDataAmount(ingredient.item, ingredient.amount))
                return false;
        }

        return true;
    }

    public void Cook(Recipe recipe)
    {
        if (recipe == null) return;

        if (cookedRecipes.Contains(recipe.id))
        {
            Debug.Log("Recipe already cooked in this session");
            return;
        }

        if (!CanCook(recipe, playerInventory))
        {
            Debug.Log("Missing ingredients");
            return;
        }

        foreach (var ingredient in recipe.ingredients)
            playerInventory.RemoveItemDataAmount(ingredient.item, ingredient.amount);

        playerInventory.AddCookingDishToInventory(recipe.result);

        cookedRecipes.Add(recipe.id);

        if (recipeProcessManager != null)
            recipeProcessManager.OnRecipeCooked(recipe, cookBook);

        if (cookBook != null)
            cookBook.CloseCookBook();
    }
}