using UnityEngine;

public enum ItemType
{
    Metal, Wood, Stone
}

public class RewardItem : InventoryItem
{
    [Header("Item Settings")]
    public ItemType type;
    public int Value;

    [Header("Workbench Control")]
    public bool isUpgraded = false;
    public bool canBeUpgrated = true;

    public Material upgradedMat;

    private MeshRenderer[] renderers;

    protected override void Awake()
    {
        base.Awake();
        renderers = GetComponentsInChildren<MeshRenderer>(true);

        Debug.Log($"[{name}] Found MeshRenderers: {renderers.Length}");
        for (int i = 0; i < renderers.Length; i++)
            Debug.Log($"[{name}] Renderer[{i}] = {GetFullPath(renderers[i].transform)} enabled={renderers[i].enabled}");
    }

    public void ApplyUpgrade()
    {
        Debug.Log($"[ApplyUpgrade] {name} called. isUpgraded={isUpgraded}, canBeUpgrated={canBeUpgrated}");

        if (isUpgraded) return;

        if (!canBeUpgrated)
        {
            Debug.LogWarning($"[ApplyUpgrade] {name} cannot be upgraded.");
            return;
        }

        isUpgraded = true;
        UpdateVisual();
        Debug.Log("Item upgraded! New Texture!!");
    }


    public void UpdateVisual()
    {
        if (renderers == null || renderers.Length == 0)
        {
            renderers = GetComponentsInChildren<MeshRenderer>(true);
        }

        if (renderers == null || renderers.Length == 0)
        {
            Debug.LogError($"[{name}] No MeshRenderers found!");
            return;
        }

        if (!isUpgraded)
        {
            Debug.Log($"[{name}] Item is not upgraded. Keeping prefab material.");
            return;
        }

        if (upgradedMat == null)
        {
            Debug.LogError($"[{name}] upgradedMat is NULL!");
            return;
        }

        foreach (MeshRenderer r in renderers)
        {
            if (r == null) continue;
            // якщо хтось юзає MaterialPropertyBlock (outline/hover), це може перебивати вигляд
            r.SetPropertyBlock(null);

            Material[] mats = r.sharedMaterials;

            if (mats == null || mats.Length == 0)
                continue;

            mats[0] = upgradedMat;
            r.sharedMaterials = mats;
        }
    }

    private static string GetFullPath(Transform t)
    {
        string path = t.name;
        while (t.parent != null) { t = t.parent; path = t.name + "/" + path; }
        return path;
    }

}
