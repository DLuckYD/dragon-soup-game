using System;
using System.Collections.Generic;
using UnityEngine;

public class RecipeProgressManager : MonoBehaviour
{
    public static event Action<string> OnSuccessfulUnlock;
    public static event Action<Recipe> OnCurrentRecipeChanged;

    private Dictionary<string, UpgradeStation> stationsById = new Dictionary<string, UpgradeStation>();
    private Dictionary<string, RoomDoor> doorsById = new Dictionary<string, RoomDoor>();
    private Recipe currentActiveRecipe;

    public Recipe CurrentActiveRecipe => currentActiveRecipe;

    private string unlockMessage = "New content unlocked";

    public void Awake()
    {
        RegisterStations();
        RegisterDoors();
    }

    public void SetCurrentRecipe(Recipe recipe)
    {
        if (recipe == null)
        {
            Debug.LogWarning("[RECIPE PROGRESSION] Cannot set current recipe. Recipe is null.");
            return;
        }

        currentActiveRecipe = recipe;

        Debug.Log("[RECIPE PROGRESSION] Active recipe set to: " + recipe.displayName);

        OnCurrentRecipeChanged?.Invoke(recipe);
    }

    private void RegisterStations()
    {
        stationsById.Clear();
        UpgradeStation[] stations = FindObjectsOfType<UpgradeStation>();

        foreach (UpgradeStation station in stations)
        {
            if(string.IsNullOrEmpty(station.GetStationId))
            {
                Debug.LogWarning($"Station {station.gameObject.name} has an empty ID and will not be registered.");
                continue;
            }
            if (!stationsById.ContainsKey(station.GetStationId))
            {
                stationsById.Add(station.GetStationId, station);
            }
            else
            {
                Debug.LogWarning($"Duplicate station ID found: {station.GetStationId}. Station {station.gameObject.name} will not be registered.");
            }
        }
    }

    private void RegisterDoors()
    {
        doorsById.Clear();
        RoomDoor[] doors = FindObjectsOfType<RoomDoor>();

        foreach (RoomDoor door in doors)
        {
            if (string.IsNullOrEmpty(door.GetDoorId))
            {
                Debug.LogWarning($"Door {door.gameObject.name} has an empty ID and will not be registered.");
                continue;
            }
            if (!doorsById.ContainsKey(door.GetDoorId))
            {
                doorsById.Add(door.GetDoorId, door);
            }
            else
            {
                Debug.LogWarning($"Duplicate door ID found: {door.GetDoorId}. Door {door.gameObject.name} will not be registered.");
            }
        }
    }

    
    public void OnRecipeCooked(Recipe recipe, CookbookUI cookBook)
    {
        // Update the cookbook UI with the newly cooked recipe
        if (cookBook != null)
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
                        OnSuccessfulUnlock?.Invoke(unlockMessage);

                        currentActiveRecipe = cookBook.GetRecipeById(targetId);
                        Debug.Log("[RECIPE PROGRESSION] Active recipe set to: " + recipe.displayName);

                        if (currentActiveRecipe != null)
                        {

                            if (QuestManager.Instance != null)
                            {
                                QuestManager.Instance.SetCurrentActiveRecipe(currentActiveRecipe);
                            }
                        }
                    }

                    if (effect.effectType == EffectType.UnlockUpdateStation)
                    {
                        targetId = effect.targetId;
                        if (stationsById.TryGetValue(targetId, out UpgradeStation station))
                        {
                            station.SetInteractable(true);
                            OnSuccessfulUnlock?.Invoke(unlockMessage);
                        }
                        else
                        {
                            Debug.LogWarning($"No station found with ID: {targetId} to unlock.");
                        }
                    }

                    if (effect.effectType == EffectType.UnlockRoom)
                    {
                        targetId = effect.targetId;
                        if (doorsById.TryGetValue(targetId, out RoomDoor door))
                        {
                            door.UnlockDoor();
                            OnSuccessfulUnlock?.Invoke(unlockMessage);
                        }
                        else
                        {
                            Debug.LogWarning($"No room found with ID: {targetId} to unlock.");
                        }
                    }
                }
            }
        }
    }
}
