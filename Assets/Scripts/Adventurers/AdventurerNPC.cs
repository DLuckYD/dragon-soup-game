using UnityEngine;

public class AdventurerNPC : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private AdventurerData data;

    [Header("Visual Root")]
    [SerializeField] private GameObject visualRoot;

    [Header("Modular Visual Renderers")]
    [SerializeField] private SpriteRenderer bodyRenderer;
    [SerializeField] private SpriteRenderer headRenderer;
    [SerializeField] private SpriteRenderer weaponRenderer;

    [Header("Legacy Visual Fallback")]
    [SerializeField] private SpriteRenderer legacyRenderer;

    [Header("Interaction")]
    [SerializeField] private Collider interactionCollider;

    [Header("State")]
    [SerializeField] private AdventurerState state = AdventurerState.Offered;

    public QuestManager questManager;
    public QuestUI questUI;

    public AdventurerData Data => data;
    public AdventurerState State => state;

    private void Awake()
    {
        if (visualRoot == null)
            Debug.LogError("VisualRoot is not assigned!", this);

        if (interactionCollider == null)
            interactionCollider = GetComponent<Collider>();

        if (questManager == null)
            questManager = FindFirstObjectByType<QuestManager>();

        if (questUI == null)
            questUI = FindFirstObjectByType<QuestUI>();

        // Fallback for old prefab structure.
        // If legacyRenderer is not assigned manually, try to find an old SpriteRenderer.
        if (legacyRenderer == null && visualRoot != null)
            legacyRenderer = visualRoot.GetComponent<SpriteRenderer>();
    }

    public void Initialize(AdventurerData adventurerData, QuestManager questManager, QuestUI questUI)
    {
        data = adventurerData;
        this.questManager = questManager;
        this.questUI = questUI;

        ApplyVisual();

        SetState(AdventurerState.Offered);
        ShowAdventurer();
    }

    public void SetState(AdventurerState state)
    {
        this.state = state;
    }

    public void ShowAdventurer()
    {
        gameObject.SetActive(true);

        if (visualRoot != null)
            visualRoot.SetActive(true);

        if (interactionCollider != null)
            interactionCollider.enabled = true;
    }

    public void HideAdventurer()
    {
        if (visualRoot != null)
            visualRoot.SetActive(false);

        if (interactionCollider != null)
            interactionCollider.enabled = false;
    }

    public void Interact(PlayerInteraction player)
    {
        if (data == null)
        {
            Debug.LogWarning("AdventurerNPC has no adventurer data assigned.", this);
            return;
        }

        if (questManager == null)
        {
            Debug.LogWarning("AdventurerNPC has no quest manager assigned.", this);
            return;
        }

        switch (state)
        {
            case AdventurerState.Offered:
                if (questUI != null)
                    questUI.OpenOfferUIPanel(this);
                break;

            case AdventurerState.InProgress:
                Debug.Log($"{data.displayName} is currently on a quest.");
                break;

            case AdventurerState.WaitingReward:
                if (player == null)
                    return;

                if (player.getHeldItem() == null)
                {
                    questManager.TryOpenReturnUI(this);
                }
                else
                {
                    questManager.TrySubmitReward(this, player);
                }
                break;
        }
    }

    public void SetInteractionEnabled(bool enabled)
    {
        if (interactionCollider != null)
            interactionCollider.enabled = enabled;
    }

    private void ApplyVisual()
    {
        if (data == null)
        {
            Debug.LogWarning("AdventurerNPC: Data is missing. Cannot apply visual.", this);
            ClearModularVisual();
            return;
        }

        bool hasModularVisual =
            data.bodySprite != null ||
            data.faceSprite != null ||
            data.weaponSprite != null;

        if (hasModularVisual)
        {
            ApplyModularVisual();
            HideLegacyVisual();

            Debug.Log(
                $"[ADVENTURER VISUAL] Applied modular visual for {data.displayName}. " +
                $"Body: {(data.bodySprite != null ? data.bodySprite.name : "NULL")}, " +
                $"Head: {(data.faceSprite != null ? data.faceSprite.name : "NULL")}, " +
                $"Weapon: {(data.weaponSprite != null ? data.weaponSprite.name : "NULL")}",
                this
            );

            return;
        }

        ApplyLegacyVisual();
    }

    private void ApplyModularVisual()
    {
        if (bodyRenderer != null)
        {
            bodyRenderer.gameObject.SetActive(data.bodySprite != null);
            bodyRenderer.sprite = data.bodySprite;
        }

        if (headRenderer != null)
        {
            headRenderer.gameObject.SetActive(data.faceSprite != null);
            headRenderer.sprite = data.faceSprite;
        }

        if (weaponRenderer != null)
        {
            weaponRenderer.gameObject.SetActive(data.weaponSprite != null);
            weaponRenderer.sprite = data.weaponSprite;
        }
    }

    private void ApplyLegacyVisual()
    {
        ClearModularVisual();

        if (legacyRenderer == null)
        {
            Debug.LogWarning(
                $"AdventurerNPC: No modular sprites and no legacy renderer for {data.displayName}.",
                this
            );
            return;
        }

        if (data.skinSprite == null)
        {
            Debug.LogWarning(
                $"AdventurerNPC: No modular sprites and no legacy skinSprite for {data.displayName}.",
                this
            );

            legacyRenderer.gameObject.SetActive(false);
            return;
        }

        legacyRenderer.gameObject.SetActive(true);
        legacyRenderer.sprite = data.skinSprite;

        Debug.Log($"[ADVENTURER VISUAL] Applied legacy visual for {data.displayName}.", this);
    }

    private void ClearModularVisual()
    {
        if (bodyRenderer != null)
        {
            bodyRenderer.sprite = null;
            bodyRenderer.gameObject.SetActive(false);
        }

        if (headRenderer != null)
        {
            headRenderer.sprite = null;
            headRenderer.gameObject.SetActive(false);
        }

        if (weaponRenderer != null)
        {
            weaponRenderer.sprite = null;
            weaponRenderer.gameObject.SetActive(false);
        }
    }

    private void HideLegacyVisual()
    {
        if (legacyRenderer != null)
        {
            legacyRenderer.sprite = null;
            legacyRenderer.gameObject.SetActive(false);
        }
    }
}