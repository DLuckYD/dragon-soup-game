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
    [SerializeField] private string useSoundEventName;

    [Header("Messages")]
    [SerializeField] private string successMessage = "Item modified";
    [SerializeField] private string noItemMessage = "No item to modify";

    [Header("Conditions")]
    [SerializeField] private StationConditionSettings conditions = new StationConditionSettings();

    [Header("Effects")]
    [SerializeField] private List<ItemEffect> effects = new List<ItemEffect>();

    public bool CanInteract => canInteract;
    public string GetStationId => stationId;
    public bool LastProcessSuccessful { get; private set; }

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

        if (!conditions.IsValid(item, out string failMessage))
        {
            OnUnsuccessfulUpgrade?.Invoke(failMessage);
            Debug.Log(failMessage);
            return item;
        }

        if (!string.IsNullOrEmpty(useSoundEventName))
        {
            WwiseAudioManager.Instance.PostEvent(useSoundEventName, gameObject);
        }

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
}