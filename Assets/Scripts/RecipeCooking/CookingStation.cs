using System.Collections.Generic;
using UnityEngine;


public class CookingStation : MonoBehaviour
{
    public List<Recipe> availableRecipes;
    public HotbarManager playerInventory;
    public CookbookUI cookBook;
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

        if (cookBook != null)
            cookBook.CloseCookBook();
    }
}
