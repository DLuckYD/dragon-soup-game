using System;
using UnityEngine;
using UnityEngine.Serialization;

public enum ItemType
{
    Metal,
    Wood,
    Stone
}

public class RewardItem : InventoryItem
{
    [Header("Item Settings")]
    public ItemType type;
    public int Value;

    [Header("Modification Control")]

    // Новое нормальное имя.
    // Через FormerlySerializedAs Unity сохранит старые значения из поля canBeUpgrated в prefab-ах.
    [FormerlySerializedAs("canBeUpgrated")]
    public bool canBeModified = true;

    // Временная совместимость со старым кодом.
    // Если где-то ещё используется item.canBeUpgrated, проект не сломается.
    [Obsolete("Use canBeModified instead.")]
    public bool canBeUpgrated
    {
        get => canBeModified;
        set => canBeModified = value;
    }

    [Header("Current Item State")]

    // В текущей версии у предмета может быть только ОДНО состояние.
    // Например: None, Upgraded, Burned, Frozen или Painted.
    [SerializeField] private ItemState currentState = ItemState.None;

    public ItemState CurrentState => currentState;

    [Header("Legacy Workbench Fields")]

    // Оставляем временно, чтобы не сломать старый код, UI или проверки.
    // Теперь это поле синхронизируется с currentState.
    public bool isUpgraded = false;

    // Оставляем временно для старого ApplyUpgrade / UpdateVisual.
    public Material upgradedMat;

    private Renderer[] renderers;

    protected override void Awake()
    {
        base.Awake();

        CacheRenderers();
    }

    // Кэшируем все MeshRenderer внутри предмета.
    // Это нужно, чтобы менять материал / цвет не только на основном объекте,
    // но и на дочерних mesh-объектах.
    
    private void CacheRenderers()
    {
        // Берём все Renderer, а не только MeshRenderer.
        // Так система будет работать и с обычными mesh-объектами, и со skinned/animated objects.
        renderers = GetComponentsInChildren<Renderer>(true);

        //Debug.Log($"[{name}] Found Renderers: {renderers.Length}");

        for (int i = 0; i < renderers.Length; i++)
        {
            //Debug.Log($"[{name}] Renderer[{i}] = {GetFullPath(renderers[i].transform)} enabled={renderers[i].enabled}");
        }
    }
    
    // Добавляет или отнимает value.
    // Например Workbench будет вызывать AddValue(+5).
    public void AddValue(int amount)
    {
        Value += amount;

        Debug.Log($"[{name}] Value changed by {amount}. New value = {Value}");
    }

    // Проверяет, находится ли предмет в конкретном состоянии.
    // Например: item.HasState(ItemState.Frozen)
    public bool HasState(ItemState state)
    {
        return currentState == state;
    }

    // Проверяет, имеет ли предмет ЛЮБОЕ состояние кроме None.
    // Это важно для вашей текущей логики:
    // если предмет уже Upgraded/Frozen/Burned/Painted, другая станция его не обрабатывает.
    public bool HasAnyState()
    {
        return currentState != ItemState.None;
    }

    // Устанавливает одно текущее состояние предмета.
    // Важно: это НЕ AddState, потому что мы сейчас не складываем состояния.
    // Предмет не может быть одновременно Frozen + Burned + Painted.
    public void SetState(ItemState newState)
    {
        currentState = newState;

        // Legacy support.
        // Старое поле isUpgraded остаётся true только если текущее состояние Upgraded.
        isUpgraded = currentState == ItemState.Upgraded;

        Debug.Log($"[{name}] Current state set to: {currentState}");
    }

    // Сбрасывает состояние предмета.
    // Пока может не использоваться, но полезно для debug, reset, save/load или будущих механик.
    public void ClearState()
    {
        currentState = ItemState.None;
        isUpgraded = false;

        Debug.Log($"[{name}] State cleared.");
    }

    // Универсальная замена материала.
    // Теперь этим методом сможет пользоваться не только Workbench,
    // но и Fireplace, Fridge или любые другие станции.
    public void ApplyMaterial(Material material)
    {
        if (material == null)
        {
            Debug.LogWarning($"[{name}] ApplyMaterial failed: material is null.");
            return;
        }

        if (renderers == null || renderers.Length == 0)
        {
            CacheRenderers();
        }

        if (renderers == null || renderers.Length == 0)
        {
            Debug.LogError($"[{name}] ApplyMaterial failed: no MeshRenderers found.");
            return;
        }

        foreach (Renderer r in renderers)
        {
            if (r == null)
                continue;

            // Важно:
            // если на объекте был MaterialPropertyBlock, например outline/hover,
            // он может перебивать внешний вид. Поэтому перед полной заменой материала очищаем block.
            r.SetPropertyBlock(null);

            Material[] mats = r.sharedMaterials;

            if (mats == null || mats.Length == 0)
                continue;

            // Меняем первый материал.
            // Если позже у предметов будет несколько материалов, можно будет расширить эту логику.
            mats[0] = material;
            r.sharedMaterials = mats;
        }

        Debug.Log($"[{name}] Material applied: {material.name}");
    }

    // Универсальная покраска через shader property.
    // Это пригодится для Painting Station.
    //
    // По умолчанию используется "_BaseColor", что часто подходит для URP/Lit.
    // Если у вас Shader Graph с другим параметром, например "_ItemTint",
    // тогда effect сможет передать другое имя свойства.
    public void SetTint(Color color, string shaderColorProperty = "_BaseColor")
    {
        if (renderers == null || renderers.Length == 0)
        {
            CacheRenderers();
        }

        if (renderers == null || renderers.Length == 0)
        {
            Debug.LogError($"[{name}] SetTint failed: no MeshRenderers found.");
            return;
        }

        foreach (Renderer r in renderers)
        {
            if (r == null)
                continue;

            MaterialPropertyBlock block = new MaterialPropertyBlock();

            // Забираем текущий block, чтобы не стереть другие значения, если они уже были.
            r.GetPropertyBlock(block);

            block.SetColor(shaderColorProperty, color);

            r.SetPropertyBlock(block);
        }

        Debug.Log($"[{name}] Tint applied: {color}");
    }

    // Универсальное добавление particle prefab к предмету.
    // Это пригодится для fire/frozen/magic effects.
    public GameObject AttachParticles(
        GameObject particlePrefab,
        bool fitToItemBounds = true,
        float sizeMultiplier = 1.0f,
        float verticalOffset = 0f,
        bool clampRadius = true,
        float minRadius = 0.35f,
        float maxRadius = 0.6f
    )
    {
        if (particlePrefab == null)
        {
            Debug.LogWarning($"[{name}] AttachParticles failed: particle prefab is null.");
            return null;
        }

        Bounds bounds = GetCombinedRendererBounds();

        GameObject particles = Instantiate(particlePrefab, transform);

        // Bounds.center находится в world space.
        // Переводим центр bounds в local space предмета.
        Vector3 localCenter = transform.InverseTransformPoint(bounds.center);
        localCenter.y += verticalOffset;

        particles.transform.localPosition = localCenter;
        particles.transform.localRotation = Quaternion.identity;
        particles.transform.localScale = Vector3.one;

        if (fitToItemBounds)
        {
            FitParticleSystemsToBounds(
                particles,
                bounds,
                sizeMultiplier,
                clampRadius,
                minRadius,
                maxRadius
            );
        }

        Debug.Log($"[{name}] Particles attached: {particlePrefab.name}");

        return particles;
    }
    
    public Bounds GetCombinedRendererBounds()
    {
        if (renderers == null || renderers.Length == 0)
        {
            CacheRenderers();
        }

        if (renderers == null || renderers.Length == 0)
        {
            // fallback, если у предмета почему-то нет Renderer
            return new Bounds(transform.position, Vector3.one * 0.5f);
        }

        bool hasBounds = false;
        Bounds combinedBounds = new Bounds(transform.position, Vector3.zero);

        foreach (Renderer r in renderers)
        {
            if (r == null)
                continue;

            // Не учитываем particle systems в размере самого предмета.
            // Иначе particles могут увеличивать bounds предмета.
            if (r is ParticleSystemRenderer)
                continue;

            if (!r.enabled)
                continue;

            if (!hasBounds)
            {
                combinedBounds = r.bounds;
                hasBounds = true;
            }
            else
            {
                combinedBounds.Encapsulate(r.bounds);
            }
        }

        if (!hasBounds)
        {
            return new Bounds(transform.position, Vector3.one * 0.5f);
        }

        return combinedBounds;
    }
    
    private void FitParticleSystemsToBounds(
    GameObject particlesObject,
    Bounds bounds,
    float sizeMultiplier,
    bool clampRadius,
    float minRadius,
    float maxRadius
)
{
    ParticleSystem[] particleSystems = particlesObject.GetComponentsInChildren<ParticleSystem>(true);

    if (particleSystems == null || particleSystems.Length == 0)
    {
        Debug.LogWarning($"[{name}] No ParticleSystems found on particle prefab.");
        return;
    }

    Vector3 worldSize = bounds.size * sizeMultiplier;

    // Старый вариант:
    // берёт самую длинную сторону объекта.
    // Для трубы/ножки стула это даёт огромный radius.
    float rawWorldRadius = Mathf.Max(worldSize.x, worldSize.y, worldSize.z) * 0.5f;

    // Новый вариант:
    // ограничиваем радиус, чтобы вытянутые объекты не получали гигантскую сферу.
    float finalWorldRadius = rawWorldRadius;

    if (clampRadius)
    {
        finalWorldRadius = Mathf.Clamp(rawWorldRadius, minRadius, maxRadius);
    }

    float localRadius = finalWorldRadius;

    foreach (ParticleSystem ps in particleSystems)
    {
        if (ps == null)
            continue;

        ParticleSystem.MainModule main = ps.main;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;

        ParticleSystem.ShapeModule shape = ps.shape;

        if (shape.shapeType == ParticleSystemShapeType.Sphere ||
            shape.shapeType == ParticleSystemShapeType.Hemisphere ||
            shape.shapeType == ParticleSystemShapeType.Circle)
        {
            shape.radius = localRadius;
        }

        Debug.Log(
            $"[{name}] Particle fit: boundsSize={bounds.size}, rawRadius={rawWorldRadius}, finalRadius={finalWorldRadius}, localRadius={localRadius}"
        );
    }
}

    // Legacy method.
    // Оставляем для старого WorkbenchStation, если он ещё где-то используется.
    // Но внутри он теперь работает через новую систему CurrentState.
    public void ApplyUpgrade()
    {
        Debug.Log($"[ApplyUpgrade] {name} called. CurrentState={currentState}, canBeModified={canBeModified}");

        // В новой логике нельзя улучшать предмет, если он уже имеет любое состояние.
        // Например, если он Frozen или Burned, он уже не должен стать Upgraded.
        if (HasAnyState())
        {
            Debug.LogWarning($"[ApplyUpgrade] {name} already has state: {currentState}");
            return;
        }

        if (!canBeModified)
        {
            Debug.LogWarning($"[ApplyUpgrade] {name} cannot be modified.");
            return;
        }

        SetState(ItemState.Upgraded);

        if (upgradedMat != null)
        {
            ApplyMaterial(upgradedMat);
        }
        else
        {
            Debug.LogWarning($"[{name}] upgradedMat is null. State was changed, but material was not applied.");
        }

        Debug.Log("Item upgraded! New Texture!!");
    }

    // Legacy method.
    // Оставляем, потому что старый код мог вызывать UpdateVisual().
    // Сейчас он просто применяет upgradedMat, если предмет находится в состоянии Upgraded.
    public void UpdateVisual()
    {
        if (!HasState(ItemState.Upgraded))
        {
            Debug.Log($"[{name}] Item is not upgraded. CurrentState={currentState}. Keeping current material.");
            return;
        }

        if (upgradedMat == null)
        {
            Debug.LogError($"[{name}] upgradedMat is NULL!");
            return;
        }

        ApplyMaterial(upgradedMat);
    }

    private static string GetFullPath(Transform t)
    {
        string path = t.name;

        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }

        return path;
    }
}