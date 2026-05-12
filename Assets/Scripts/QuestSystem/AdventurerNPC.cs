using UnityEngine;


public class AdventurerNPC : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private AdventurerData data;
    
    [Header("Visual")]
    [SerializeField] private GameObject visualRoot;
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
    }


    public void SetSprite(Sprite sprite)
    {
        if (visualRoot == null) return;

        var sr = visualRoot.GetComponent<SpriteRenderer>();
        if (sr == null)
            sr = visualRoot.GetComponentInChildren<SpriteRenderer>(true); // true = включает неактивных

        if (sr != null)
            sr.sprite = sprite;
    }


    public void Initialize(AdventurerData adventurerData, QuestManager questManager, QuestUI questUI)
    {
        data = adventurerData;
        this.questManager = questManager;
        this.questUI = questUI;

        // ✅ ставим спрайт из Data
        if (data != null && data.skinSprite != null)
            SetSprite(data.skinSprite);
        else
            Debug.LogWarning("AdventurerNPC: Data or skinSprite is missing, sprite not set.", this);

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
            Debug.LogWarning($"AdventurerNPC: has no adventurer data assigned");
        }

        if (questManager == null)
        {
            Debug.LogWarning($"AdventurerNPC: has no quest manager assigned");
        }

        switch (state)
        {
            case AdventurerState.Offered:
                questUI.OpenOfferUIPanel(this);
                break;
            
            case AdventurerState.InProgress:
                Debug.Log($"{data.displayName} is currently on a quest ");
                break;

            case AdventurerState.WaitingReward:
                if (player == null)
                    return;

                // if there is no held item show Return UI
                if (player.getHeldItem() == null)
                {
                    questManager.TryOpenReturnUI(this);
                }
                else
                {
                    // if there is a held item try to submit reward
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
    
}
