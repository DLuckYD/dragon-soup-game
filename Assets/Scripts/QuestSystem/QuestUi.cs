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

    [Header("Return: Feedback")]
    [SerializeField] private TMP_Text returnFeedbackText;

    [Header("Return: Buttons")]
    [SerializeField] private Button okayOneMinuteButton;
    [SerializeField] private Button goAwayButton;
    [SerializeField] private Button rollHaggleButton;

    private QuestManager questManager;
    private AdventurerNPC currentNpc;

    public FirstPersonCamera cameraScript;

    public void Initialize(QuestManager qm)
    {
        questManager = qm;

        if (giveQuestButton != null)
            giveQuestButton.onClick.AddListener(OnGiveQuestClicked);

        if (abandonOfferButton != null)
            abandonOfferButton.onClick.AddListener(OnOfferAbandonPressed);

        if (okayOneMinuteButton != null)
            okayOneMinuteButton.onClick.AddListener(CloseAll);

        if (goAwayButton != null)
            goAwayButton.onClick.AddListener(OnGoAwayClicked);

        if (rollHaggleButton != null)
            rollHaggleButton.onClick.AddListener(OnRollHaggleClicked);

        CloseAll();
    }

    public void OnOfferAbandonPressed()
    {
        Debug.Log($"[QuestUI] Offer Abandon pressed currentNpc={(currentNpc ? currentNpc.name : "NULL")}", this);

        if (questManager == null || currentNpc == null)
            return;

        questManager.DismissNpc(currentNpc);
    }

    // --- First dialog: adventurer offers quest ---
    public void OpenOfferUIPanel(AdventurerNPC npc)
    {
        currentNpc = npc;

        if (offerPanel != null)
            offerPanel.SetActive(true);

        if (returnPanel != null)
            returnPanel.SetActive(false);

        SetReturnFeedback("");
        SetRollHaggleButtonVisible(false);

        FreezeGame();

        QuestManager.OfferPreview preview = questManager.GetOrCreateOfferPreview(npc);

        if (adventurerImage != null)
        {
            Sprite sprite = npc != null && npc.Data != null ? npc.Data.skinSprite : null;

            adventurerImage.enabled = sprite != null;
            adventurerImage.sprite = sprite;
        }

        if (offerIntroText != null)
            offerIntroText.text = preview.intro;

        if (offerOutroText != null)
            offerOutroText.text = preview.outro;

        if (offerIngredientLine != null)
            offerIngredientLine.text = $"{preview.ingredientName} x{preview.amount}";

        if (offerIngredientIcon != null)
        {
            offerIngredientIcon.enabled = preview.ingredientIcon != null;
            offerIngredientIcon.sprite = preview.ingredientIcon;
        }
    }

    // --- Second dialog: adventurer returned and asks for reward ---
    public void OpenReturnUI(AdventurerNPC npc, QuestManager.ReturnInfo info)
    {
        currentNpc = npc;

        if (offerPanel != null)
            offerPanel.SetActive(false);

        if (returnPanel != null)
            returnPanel.SetActive(true);

        FreezeGame();

        SetRollHaggleButtonVisible(false);
        SetReturnFeedback("");

        string preferredRewardItemId = "Unknown";
        string preferredItemState = "None";

        if (npc != null && npc.Data != null)
        {
            if (!string.IsNullOrEmpty(npc.Data.preferredRewardItemId))
                preferredRewardItemId = npc.Data.preferredRewardItemId;

            preferredItemState = npc.Data.preferredItemState.ToString();
        }

        if (returnMainText != null)
        {
            returnMainText.text =
                $"I brought {info.ingredientName} x{info.amount}.\n\n" +
                $"Preferred reward: {preferredRewardItemId}\n" +
                $"Preferred state: {preferredItemState}\n\n" +
                $"Difficulty Level: {info.difficultyLevel}\n" +
                $"Haggle attempts left: {info.haggleAttemptsLeft}\n\n" +
                $"Hold a reward item and submit it to the adventurer.";
        }
    }

    public void ShowRewardTradeFeedback(RewardTradeSubmitResult result)
    {
        switch (result.outcome)
        {
            case RewardTradeSubmitOutcome.Accepted:
                SetRollHaggleButtonVisible(false);

                SetReturnFeedback(
                    "✓ The adventurer accepts this reward.\n" +
                    $"Score: {result.evaluation.score} / Difficulty: {result.evaluation.difficulty}"
                );
                break;

            case RewardTradeSubmitOutcome.HaggleRequired:
                SetRollHaggleButtonVisible(true);

                SetReturnFeedback(
                    "? The adventurer is unsure.\n" +
                    "You can roll D20 to haggle.\n" +
                    $"Score: {result.evaluation.score} / Difficulty: {result.evaluation.difficulty}\n" +
                    $"Attempts left: {result.haggleAttemptsLeft}"
                );
                break;

            case RewardTradeSubmitOutcome.Refused:
                SetRollHaggleButtonVisible(false);

                SetReturnFeedback(
                    "✗ The adventurer refuses this reward.\n" +
                    "Try another reward item.\n" +
                    $"Score: {result.evaluation.score} / Difficulty: {result.evaluation.difficulty}"
                );
                break;
        }
    }

    public void ShowHaggleRollFeedback(HaggleRollResult result)
    {
        switch (result.outcome)
        {
            case HaggleRollOutcome.Success:
                SetRollHaggleButtonVisible(false);

                SetReturnFeedback(
                    $"✓ Haggle succeeded!\n" +
                    $"D20 roll: {result.roll}\n" +
                    "The adventurer accepts the reward."
                );
                break;

            case HaggleRollOutcome.FailedCanRetry:
                SetRollHaggleButtonVisible(true);

                SetReturnFeedback(
                    $"✗ Haggle failed.\n" +
                    $"D20 roll: {result.roll}\n" +
                    $"Attempts left: {result.attemptsLeft}\n" +
                    "You can roll one more time."
                );
                break;

            case HaggleRollOutcome.FailedNoAttemptsLeft:
                SetRollHaggleButtonVisible(false);

                SetReturnFeedback(
                    $"✗ Haggle failed.\n" +
                    $"D20 roll: {result.roll}\n" +
                    "No attempts left. The adventurer leaves."
                );
                break;

            case HaggleRollOutcome.NoPendingHaggle:
                SetRollHaggleButtonVisible(false);

                SetReturnFeedback(
                    "There is no pending haggle item.\n" +
                    "Submit a suitable reward item first."
                );
                break;
        }
    }

    public void CloseAll()
    {
        if (offerPanel != null)
            offerPanel.SetActive(false);

        if (returnPanel != null)
            returnPanel.SetActive(false);

        SetReturnFeedback("");
        SetRollHaggleButtonVisible(false);

        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (cameraScript != null)
            cameraScript.canLook = true;

        currentNpc = null;
    }

    private void OnGiveQuestClicked()
    {
        if (questManager == null || currentNpc == null)
            return;

        questManager.AcceptQuest(currentNpc);
        CloseAll();
    }

    private void OnGoAwayClicked()
    {
        if (questManager == null || currentNpc == null)
            return;

        // The player refused to give a reward.
        // The adventurer leaves and the quest fails.
        questManager.RejectReturnedAdventurer(currentNpc);

        CloseAll();
    }

    private void OnRollHaggleClicked()
    {
        if (questManager == null || currentNpc == null)
            return;

        questManager.TryRollHaggle(currentNpc);
    }

    private void SetReturnFeedback(string text)
    {
        if (returnFeedbackText != null)
            returnFeedbackText.text = text;
    }

    private void SetRollHaggleButtonVisible(bool visible)
    {
        if (rollHaggleButton == null)
            return;

        rollHaggleButton.gameObject.SetActive(visible);
        rollHaggleButton.interactable = visible;
    }

    private void FreezeGame()
    {
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (cameraScript != null)
            cameraScript.canLook = false;
    }
}