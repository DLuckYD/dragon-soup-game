using System;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeStation : MonoBehaviour
{
    public static event Action<string> OnSuccessfulUpgrade;
    public static event Action<string> OnUnsuccessfulUpgrade;

    [Header("Station Settings")]
    [SerializeField] private string stationId;

    [Header("Station State")]
    [SerializeField] private bool canInteract = true;
    [SerializeField] private string disabledMessage = "Station is locked";

    [Header("Audio")]
    [SerializeField] private string WwiseSwitchName;

    [Header("Messages")]
    [SerializeField] private string successMessage = "Item modified";
    [SerializeField] private string noItemMessage = "No item to modify";

    [Header("Destroy Rules")]
    [Tooltip("If true, this station will destroy items listed below before applying normal effects.")]
    [SerializeField] private bool destroyListedItems = false;

    [Tooltip("Concrete IngredientData items that this station should destroy.")]
    [SerializeField] private List<IngredientData> itemsToDestroy = new List<IngredientData>();

    [SerializeField] private string itemDestroyedMessage = "Item destroyed";

    [Header("Conditions")]
    [SerializeField] private StationConditionSettings conditions = new StationConditionSettings();

    [Header("Effects")]
    [SerializeField] private List<ItemEffect> effects = new List<ItemEffect>();

    public bool CanInteract => canInteract;
    public string GetStationId => stationId;

    public bool LastProcessSuccessful { get; private set; }

    // This is useful for PlayerInteraction to understand that result == null
    // happened because the station destroyed the item.
    public bool LastItemWasDestroyed { get; private set; }

    public void SetInteractable(bool value)
    {
        canInteract = value;
    }

    public virtual RewardItem UpgradeItem(RewardItem item)
    {
        return ProcessItem(item);
    }

    public virtual RewardItem ProcessItem(RewardItem item)
    {
        LastProcessSuccessful = false;
        LastItemWasDestroyed = false;

        if (!canInteract)
        {
            OnUnsuccessfulUpgrade?.Invoke(disabledMessage);
            Debug.Log(disabledMessage);
            return item;
        }

        if (item == null)
        {
            OnUnsuccessfulUpgrade?.Invoke(noItemMessage);
            Debug.Log(noItemMessage);
            return null;
        }

        // IMPORTANT:
        // Destroy check happens before normal conditions and effects.
        //
        // Example:
        // Metal Pipe + Axe = destroyed.
        // In that case we do not want SetState / SetTint / Particles to run.
        if (ShouldDestroyItem(item))
        {
            PlayStationSound();

            LastProcessSuccessful = true;
            LastItemWasDestroyed = true;

            OnSuccessfulUpgrade?.Invoke(itemDestroyedMessage);

            Debug.Log($"[{name}] Destroyed item '{item.name}' with itemData '{item.itemData.name}'.");

            Destroy(item.gameObject);

            // Returning null tells PlayerInteraction that the item no longer exists.
            return null;
        }

        if (!conditions.IsValid(item, out string failMessage))
        {
            OnUnsuccessfulUpgrade?.Invoke(failMessage);
            Debug.Log(failMessage);
            return item;
        }

        PlayStationSound();

        foreach (ItemEffect effect in effects)
        {
            if (effect == null)
                continue;

            effect.Apply(item, this);
        }

        LastProcessSuccessful = true;

        OnSuccessfulUpgrade?.Invoke(successMessage);
        Debug.Log(successMessage);

        return item;
    }

    private bool ShouldDestroyItem(RewardItem item)
    {
        if (!destroyListedItems)
            return false;

        if (item == null)
            return false;

        if (item.itemData == null)
        {
            Debug.LogWarning($"[{name}] Cannot check destroy list because item '{item.name}' has null itemData.");
            return false;
        }

        if (itemsToDestroy == null || itemsToDestroy.Count == 0)
            return false;

        return itemsToDestroy.Contains(item.itemData);
    }

    private void PlayStationSound()
    {
        if (string.IsNullOrEmpty(WwiseSwitchName))
            return;

        WwiseAudioManager.Instance.SetSwitchValue("Upgrade_Station", WwiseSwitchName, gameObject);
        WwiseAudioManager.Instance.PostEvent("Upgrade_Station_Success", gameObject);
    }
}