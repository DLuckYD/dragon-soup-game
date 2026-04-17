using UnityEngine;

public enum ItemType
{
    Chair, Matal, Wood, Stone
}

public class RewardItem : InventoryItem
{
    [Header("Item Settings")]
    public ItemType type;
    public int Value;

    [Header("Workbench Control")]
    public bool isUpgraded = false;
    public bool canBeUpgrated = true;

    [Header("Visuals (same matherials for all objects)")]
    public Material defaultMat;
    public Material upgradedMat;

    private MeshRenderer[] renderers;


    private void Awake()
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

        isUpgraded = true;
        UpdateVisual();
        Debug.Log("Item upgraded! New Texture!!");
    }


    public void UpdateVisual()
    {
        Material target = isUpgraded ? upgradedMat : defaultMat;

        if (target == null)
        {
            Debug.LogError($"[{name}] Target material is NULL! isUpgraded={isUpgraded}");
            return;
        }

        if (renderers == null || renderers.Length == 0)
        {
            Debug.LogError($"[{name}] No MeshRenderers found!");
            return;
        }

        foreach (var r in renderers)
        {
            // якщо хтось юзає MaterialPropertyBlock (outline/hover), це може перебивати вигляд
            r.SetPropertyBlock(null);

            var mats = r.sharedMaterials;
            if (mats != null && mats.Length > 0)
            {
                Debug.Log($"[{name}] Swap on {GetFullPath(r.transform)} slot0: {mats[0]?.name} -> {target.name}");
                mats[0] = target;
                r.sharedMaterials = mats;
            }
        }
    }

    private static string GetFullPath(Transform t)
    {
        string path = t.name;
        while (t.parent != null) { t = t.parent; path = t.name + "/" + path; }
        return path;
    }

}
