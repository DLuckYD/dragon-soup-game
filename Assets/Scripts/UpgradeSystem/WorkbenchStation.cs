using UnityEngine;
using System;

public class WorkbenchStation : UpgradeStation
{
    public static WorkbenchStation Instance { get; private set; }

    public static event Action<string> OnSuccessfulUpgrade;
    
    public static event Action<string> OnUnsuccessfulUpgrade;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public override RewardItem UpgradeItem(RewardItem item)
    {
        if (item != null && !item.canBeUpgrated)
        {
            Destroy(item.gameObject);
            OnUnsuccessfulUpgrade?.Invoke("Item is destroyed ");
            return null;
        }
        else
        {
            AkUnitySoundEngine.PostEvent("Workbench_Use", gameObject);
            item.Value += 5;
            item.ApplyUpgrade();
            OnSuccessfulUpgrade?.Invoke("Item upgraded ");
        }
        return item;
    }
}
