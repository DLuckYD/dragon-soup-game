using UnityEngine;

[CreateAssetMenu(menuName = "Upgrade System/Effects/Apply Material")]
public class ApplyMaterialEffect : ItemEffect
{
    [Header("Material Settings")]
    [SerializeField] private Material material;

    public override void Apply(RewardItem item, UpgradeStation station)
    {
        // Safety check.
        if (item == null)
            return;

        if (material == null)
        {
            Debug.LogWarning($"[{item.name}] ApplyMaterialEffect failed: material is null.");
            return;
        }

        // Просим RewardItem заменить материал на всех MeshRenderer.
        item.ApplyMaterial(material);

        Debug.Log($"[{item.name}] ApplyMaterialEffect applied. Material = {material.name}");
    }
}