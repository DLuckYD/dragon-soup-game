using UnityEngine;

[CreateAssetMenu(menuName = "Upgrade System/Effects/Attach Particles")]
public class AttachParticlesEffect : ItemEffect
{
    [Header("Particle Prefab")]
    [SerializeField] private GameObject particlePrefab;

    [Header("Auto Fit")]
    [SerializeField] private bool fitToItemBounds = true;

    // Общий множитель размера эффекта.
    // Для Workbench sparkles лучше держать около 1.
    [SerializeField] private float sizeMultiplier = 1.0f;

    [Header("Radius Clamp")]

    // Если true, радиус particle shape не сможет стать слишком маленьким или слишком большим.
    [SerializeField] private bool clampRadius = true;

    // Минимальный радиус эффекта.
    [SerializeField] private float minRadius = 0.35f;

    // Максимальный радиус эффекта.
    // Для твоего случая я бы начал с 0.6.
    [SerializeField] private float maxRadius = 0.6f;

    [Header("Position")]
    [SerializeField] private float verticalOffset = 0f;

    public override void Apply(RewardItem item, UpgradeStation station)
    {
        if (item == null)
            return;

        if (particlePrefab == null)
        {
            Debug.LogWarning($"[{item.name}] AttachParticlesEffect failed: particlePrefab is null.");
            return;
        }

        item.AttachParticles(
            particlePrefab,
            fitToItemBounds,
            sizeMultiplier,
            verticalOffset,
            clampRadius,
            minRadius,
            maxRadius
        );

        Debug.Log($"[{item.name}] AttachParticlesEffect applied: {particlePrefab.name}");
    }
}