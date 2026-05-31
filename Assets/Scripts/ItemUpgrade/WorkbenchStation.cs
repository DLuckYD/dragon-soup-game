using UnityEngine;

public class WorkbenchStation : UpgradeStation
{
    public override RewardItem UpgradeItem(RewardItem item)
    {
        if (item != null && !item.canBeUpgrated)
        {
            Destroy(item.gameObject);
            return null;
        }
        else
        {
            AkUnitySoundEngine.PostEvent("Workbench_Use", gameObject);
            item.Value += 5;
            item.ApplyUpgrade();
        }
        return item;
    }
}
