using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class RecipeProgressManager : MonoBehaviour
{
    public void OnRecipeCooked(Recipe recipe, CookbookUI cookBook)
    {
        // Update the cookbook UI with the newly cooked recipe
        if(cookBook != null)
        {
            cookBook.MarkRecipeAsInteractable(recipe.id, false);
        }

        if (recipe != null)
        {
            List<ProgressionEffect> progressionEffects = recipe.progressionEffects;

            string targetId = null;
            if (progressionEffects != null)
            {
                foreach (var effect in progressionEffects)
                {
                    if (effect.effectType == EffectType.UnlockRecipe)
                    {
                        targetId = effect.targetId;
                        cookBook.MarkRecipeAsInteractable(targetId, true);
                    }
                }
            }
        }
    }
}
