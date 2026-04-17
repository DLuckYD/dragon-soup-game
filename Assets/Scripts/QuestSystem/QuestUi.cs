using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject offerPanel;
    [SerializeField] private GameObject returnPanel;

    // -------- OFFER DIALOG --------
    [Header("Adventurer Image")]
    [SerializeField] private Image adventurerImage;

    [Header("Offer: Blocks")]
    [SerializeField] private TMP_Text offerIntroText;
    [SerializeField] private TMP_Text offerOutroText;

    [Header("Offer: Goal Card")]
    [SerializeField] private Image offerIngredientIcon;
    [SerializeField] private TMP_Text offerIngredientLine; // "Milk x3"

    [Header("Offer: Buttons")]
    [SerializeField] private Button giveQuestButton;
    [SerializeField] private Button abandonOfferButton;

    // -------- RETURN DIALOG --------
    [Header("Return: Text")]
    [SerializeField] private TMP_Text returnMainText;

    [Header("Return: Buttons")]
    [SerializeField] private Button okayOneMinuteButton; 
    [SerializeField] private Button goAwayButton;      

    private QuestManager questManager;
    private AdventurerNPC currentNpc;

    public FirstPersonCamera cameraScript;


    public void Initialize(QuestManager qm)
    {
        questManager = qm;

        if (giveQuestButton != null) giveQuestButton.onClick.AddListener(OnGiveQuestClicked);
        if (abandonOfferButton != null) abandonOfferButton.onClick.AddListener(OnOfferAbandonPressed);

        if (okayOneMinuteButton != null) okayOneMinuteButton.onClick.AddListener(CloseAll);
        if (goAwayButton != null) goAwayButton.onClick.AddListener(OnGoAwayClicked);

        CloseAll();
    }

    public void OnOfferAbandonPressed()
    {
        Debug.Log($"[QuestUI] Offer Abandon pressed currentNpc={(currentNpc ? currentNpc.name : "NULL")}", this);
        questManager.DismissNpc(currentNpc);
    }

    // --- 1) First diLOG: give quest ---
    public void OpenOfferUIPanel(AdventurerNPC npc)
    {
        currentNpc = npc;

        if (offerPanel != null) offerPanel.SetActive(true);
        if (returnPanel != null) returnPanel.SetActive(false);

        var preview = questManager.GetOrCreateOfferPreview(npc);

        FreezeGame();

        if (adventurerImage != null)
        {
            adventurerImage.enabled = npc.Data.skinSprite != null;
            adventurerImage.sprite = npc.Data.skinSprite;
        }

        if (offerIntroText != null) offerIntroText.text = preview.intro;
        if (offerOutroText != null) offerOutroText.text = preview.outro;

        if (offerIngredientLine != null)
            offerIngredientLine.text = $"{preview.ingredientName} x{preview.amount}";

        if (offerIngredientIcon != null)
        {
            offerIngredientIcon.enabled = preview.ingredientIcon != null;
            offerIngredientIcon.sprite = preview.ingredientIcon;
        }
    }

    // --- 2) Second dialog: npc came back and ask for reward ---
    public void OpenReturnUI(AdventurerNPC npc, QuestManager.ReturnInfo info)
    {
        currentNpc = npc;

        if (offerPanel != null) offerPanel.SetActive(false);
        if (returnPanel != null) returnPanel.SetActive(true);

        FreezeGame();

        if (returnMainText != null)
        {
            returnMainText.text =
                $"I brought {info.ingredientName} x{info.amount}.\n" +
                $"I want a reward with value >= {info.minRewardValue}.\n" +
                $"Attempts left: {info.attemptsLeft}\n\n" +
                $"Press T with a reward item in your hands.";
        }
    }

    public void CloseAll()
    {
        if (offerPanel != null) offerPanel.SetActive(false);
        if (returnPanel != null) returnPanel.SetActive(false);

        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        cameraScript.canLook = true;

        currentNpc = null;
    }

    private void OnGiveQuestClicked()
    {
        if (questManager == null || currentNpc == null) return;
        questManager.AcceptQuest(currentNpc);
        CloseAll();
    }

    private void OnGoAwayClicked()
    {
        if (questManager == null || currentNpc == null) return;

        // The player refused to give the reward, the NPC leaves, the quest fails.
        questManager.RejectReturnedAdventurer(currentNpc);

        CloseAll();
    }

    private void FreezeGame()
    {
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        cameraScript.canLook = false;
    }
}
