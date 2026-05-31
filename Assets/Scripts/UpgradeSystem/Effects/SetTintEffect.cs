using UnityEngine;

[CreateAssetMenu(menuName = "Upgrade System/Effects/Set Tint")]
public class SetTintEffect : ItemEffect
{
    [Header("Tint Settings")]
    [SerializeField] private Color tintColor = Color.green;

    [Tooltip("For URP/Lit usually use _BaseColor. For custom Shader Graph you can use your own property, for example _ItemTint.")]
    [SerializeField] private string shaderColorProperty = "_BaseColor";

    public override void Apply(RewardItem item, UpgradeStation station)
    {
        // Safety check.
        if (item == null)
            return;

        // Применяем цвет через MaterialPropertyBlock.
        // Это не создаёт новый материал для каждого предмета.
        item.SetTint(tintColor, shaderColorProperty);

        Debug.Log($"[{item.name}] SetTintEffect applied. Color = {tintColor}");
    }
}