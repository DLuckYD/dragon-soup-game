using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private QuestUI questUI;
    [SerializeField] private HouseSpawner houseSpawner;
    [SerializeField] private HotbarManager playerInventory;
    [SerializeField] private AdventurerSpawner adventurerSpawner;
    [SerializeField] private RecipeProgressManager recipeProgressManager;
    [SerializeField] private RewardTradeManager rewardTradeManager;

    [Header("Items Database")]
    [SerializeField] private ItemDatabase itemDatabase;

    [Header("Dialogue JSON")]
    [SerializeField] private TextAsset dialogueJsonFile;

    [Header("Quest Generation (MVP)")]
    [SerializeField] private IngredientData defaultIngredient;
    [SerializeField] private int defaultAmount = 3;
    [SerializeField] private int defaultDifficultyLevel = 1;
    [SerializeField] private float defaultReturnDelaySeconds = 15f;

    [Header("Recipe Pool")]
    [SerializeField] private List<Recipe> recipePool = new();

    private Recipe currentActiveRecipe;

    // Fired when an item reward was consumed by the quest system.
    public event Action<object> OnRewardConsumed;

    // Fired when the adventurer cycle is fully finished and the NPC can be removed.
    public event Action<AdventurerNPC> OnQuestFinished;

    // Fired when a quest is accepted and the adventurer leaves the queue.
    public event Action<AdventurerNPC> OnQuestAccepted;

    // Fired when a quest timer finishes and the adventurer returns.
    public event Action<AdventurerNPC> OnAdventurerReturned;

    private QuestDialogueBankJson bank;

    // Runtime active quests. One adventurer can have one active quest.
    private readonly Dictionary<AdventurerNPC, ActiveQuest> activeQuests = new();

    // Temporary generated quest offers.
    // We block saving while offer panels are open, so this does not need to be saved for now.
    private readonly Dictionary<AdventurerNPC, OfferPreview> offerPreviews = new();

    // Bag system: quest targets are selected without repeats until the bag is exhausted.
    private readonly List<QuestTarget> targetBag = new();
    private int bagIndex = 0;

    // Temporary list used in Update to avoid modifying quest state while iterating dictionary.
    private readonly List<ActiveQuest> questsReadyToReturn = new();

    // ------------------ Data structs ------------------

    public struct OfferPreview
    {
        public string intro;
        public string outro;

        public string introId;
        public string outroId;
        public string hintId;
        public string hintText;

        public Recipe recipe;
        public IngredientData ingredientData;
        public string ingredientName;
        public Sprite ingredientIcon;
        public int amount;

        public int difficultyLevel;
        public float returnDelaySeconds;
    }

    private struct QuestTarget
    {
        public Recipe recipe;
        public IngredientData ingredient;
        public int amount;
    }

    private class ActiveQuest
    {
        public AdventurerNPC npc;

        public IngredientData ingredient;
        public int amount;
        
        // Difficulty comes from the recipe used for this quest.
        public int difficultyLevel;

        public float returnAtTime;

        public string introId;
        public string outroId;
        public string hintId;
        public string hintText;
    }

    public struct ReturnInfo
    {
        public string ingredientName;
        public int amount;
        
        public int difficultyLevel;
        public int haggleAttemptsLeft;
    }

    // ------------------ Unity ------------------

    private void Awake()
    {
        Instance = this;

        if (questUI == null)
            questUI = FindFirstObjectByType<QuestUI>();

        if (houseSpawner == null)
            houseSpawner = FindFirstObjectByType<HouseSpawner>();

        if (playerInventory == null)
            playerInventory = FindFirstObjectByType<HotbarManager>();

        if (adventurerSpawner == null)
            adventurerSpawner = FindFirstObjectByType<AdventurerSpawner>();

        if (questUI != null)
            questUI.Initialize(this);

        if (recipeProgressManager == null)
            recipeProgressManager = FindFirstObjectByType<RecipeProgressManager>();
        
        if (rewardTradeManager == null)
            rewardTradeManager = FindFirstObjectByType<RewardTradeManager>();

        LoadDialogueBank();
    }

    private void Start()
    {
        if (recipeProgressManager != null)
        {
            SetCurrentActiveRecipe(recipeProgressManager.CurrentActiveRecipe);
        }
    }

    private void Update()
    {
        questsReadyToReturn.Clear();

        foreach (var kv in activeQuests)
        {
            ActiveQuest quest = kv.Value;

            if (quest == null)
                continue;

            // The adventurer has not returned yet.
            if (quest.returnAtTime == float.MaxValue)
                continue;

            // The quest timer finished.
            if (Time.time >= quest.returnAtTime)
            {
                questsReadyToReturn.Add(quest);
            }
        }

        for (int i = 0; i < questsReadyToReturn.Count; i++)
        {
            MarkAdventurerReturned(questsReadyToReturn[i]);
        }
    }

    // ------------------ Public API ------------------

    public bool TryOpenReturnUI(AdventurerNPC npc)
    {
        if (npc == null || questUI == null)
            return false;

        if (!activeQuests.TryGetValue(npc, out ActiveQuest quest) || quest == null)
            return false;

        if (npc.State != AdventurerState.WaitingReward)
            return false;

        ReturnInfo info = new ReturnInfo
        {
            ingredientName = quest.ingredient != null ? quest.ingredient.displayName : "Ingredient",
            amount = quest.amount,
            difficultyLevel = quest.difficultyLevel,
            haggleAttemptsLeft = rewardTradeManager.GetAttemptsLeft(npc) 
        };

        questUI.OpenReturnUI(npc, info);
        return true;
    }

    public void SetCurrentActiveRecipe(Recipe recipe)
    {
        if (recipe == null)
        {
            Debug.LogWarning("[QUEST] Cannot set active recipe. Recipe is null.");
            return;
        }

        currentActiveRecipe = recipe;

        // Important: old bag was generated from previous recipe
        targetBag.Clear();
        bagIndex = 0;

        // Optional, but useful: old offer previews may contain ingredients from previous recipe
        offerPreviews.Clear();

        Debug.Log("[QUEST] Active recipe set to: " + recipe.displayName);
    }

    
    private int GetDifficultyLevelFromRecipe(Recipe recipe)
    {
        if (recipe != null)
            return Mathf.Max(1, recipe.difficultyLevel);

        if (currentActiveRecipe != null)
            return Mathf.Max(1, currentActiveRecipe.difficultyLevel);

        Debug.LogWarning("[QUEST] Recipe is missing. Using default difficulty level.");

        return Mathf.Max(1, defaultDifficultyLevel);
    }
    
    public OfferPreview GetOrCreateOfferPreview(AdventurerNPC npc)
    {
        if (npc == null)
            return default;

        if (offerPreviews.TryGetValue(npc, out OfferPreview cached))
            return cached;

        // Pick quest target from the bag system.
        QuestTarget target = PickTargetFromRecipes();

        IngredientData ingredient = target.ingredient != null ? target.ingredient : defaultIngredient;
        int amount = Mathf.Max(1, target.amount);

        if (ingredient == null)
            ingredient = defaultIngredient;

        if (amount <= 0)
            amount = Mathf.Max(1, defaultAmount);

        int difficultyLevel = GetDifficultyLevelFromRecipe(target.recipe);
        float delay = Mathf.Max(1f, defaultReturnDelaySeconds);

        // Pick dialogue lines.
        QuestDialogueEntry intro = PickRandom(bank?.intros);
        QuestDialogueEntry outro = PickRandom(bank?.outros);
        QuestDialogueEntry hint = PickRandom(bank?.hints);

        string ingredientName = ingredient != null ? ingredient.displayName : "Ingredient";
        Sprite ingredientIcon = ingredient != null ? ingredient.icon : null;
        string hintText = hint.text ?? "";

        string introText = ApplyTokens(intro.text ?? "", ingredientName, amount, hintText);

        string outroRaw = outro.text ?? "";
        string outroText = ApplyTokens(outroRaw, ingredientName, amount, hintText);

        // If the outro dialogue does not include {hint}, add the hint as a separate line.
        if (!outroRaw.Contains("{hint}", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(hintText))
            outroText = $"Hint: {hintText}\n{outroText}";

        OfferPreview preview = new OfferPreview
        {
            intro = introText,
            outro = outroText,

            introId = intro.id,
            outroId = outro.id,
            hintId = hint.id,
            hintText = hintText,

            recipe = target.recipe,
            ingredientData = ingredient,
            ingredientName = ingredientName,
            ingredientIcon = ingredientIcon,
            amount = amount,

            difficultyLevel = difficultyLevel,
            returnDelaySeconds = delay
        };

        offerPreviews[npc] = preview;

        return preview;
    }

    public void AcceptQuest(AdventurerNPC npc)
    {
        if (npc == null)
            return;

        if (activeQuests.ContainsKey(npc))
        {
            Debug.LogWarning("[QUEST] Cannot accept quest. NPC already has an active quest: " + npc.name);
            return;
        }

        OfferPreview preview = GetOrCreateOfferPreview(npc);

        ActiveQuest quest = new ActiveQuest
        {
            npc = npc,
            ingredient = preview.ingredientData != null ? preview.ingredientData : defaultIngredient,
            amount = Mathf.Max(1, preview.amount),
            difficultyLevel = Mathf.Max(1, preview.difficultyLevel),
            returnAtTime = Time.time + Mathf.Max(1f, preview.returnDelaySeconds),

            introId = preview.introId,
            outroId = preview.outroId,
            hintId = preview.hintId,
            hintText = preview.hintText
        };

        activeQuests[npc] = quest;
        offerPreviews.Remove(npc);

        npc.SetState(AdventurerState.InProgress);

        // The spawner controls queue positions and visibility.
        // If the spawner exists, let it remove this adventurer from the queue.
        if (adventurerSpawner != null)
        {
            adventurerSpawner.NotifyQuestAccepted(npc);
        }
        else
        {
            npc.HideAdventurer();
        }

        OnQuestAccepted?.Invoke(npc);

        Debug.Log(
            "[QUEST ACCEPTED] NPC: " + GetNpcDebugName(npc) +
            " | ingredient: " + (quest.ingredient != null ? quest.ingredient.id : "NULL") +
            " | amount: " + quest.amount +
            " | difficultyLevel: " + quest.difficultyLevel +
            " | returnAtTime: " + quest.returnAtTime +
            " | remaining: " + (quest.returnAtTime - Time.time) +
            " | activeQuests count: " + activeQuests.Count
        );
    }

    public void RejectReturnedAdventurer(AdventurerNPC npc)
    {
        if (npc == null)
            return;

        if (!activeQuests.TryGetValue(npc, out ActiveQuest quest) || quest == null)
            return;

        if (npc.State != AdventurerState.WaitingReward)
            return;

        FinishQuest(npc);
    }

    public void TrySubmitReward(AdventurerNPC npc, PlayerInteraction playerInteraction)
    {
        if (npc == null)
            return;

        if (!activeQuests.TryGetValue(npc, out ActiveQuest quest) || quest == null)
            return;

        if (npc.State != AdventurerState.WaitingReward)
            return;

        if (rewardTradeManager == null)
        {
            Debug.LogError("[QUEST] RewardTradeManager is missing.");
            OpenReturnUIForQuest(npc, quest);
            return;
        }

        InventoryItem heldItem = playerInteraction != null ? playerInteraction.getHeldItem() : null;

        RewardTradeSubmitResult tradeResult = rewardTradeManager.SubmitReward(
            npc,
            heldItem,
            quest.difficultyLevel
        );

        Debug.Log("[QUEST TRADE] " + tradeResult.message);

        switch (tradeResult.outcome)
        {
            case RewardTradeSubmitOutcome.Accepted:
                // RewardTradeManager decided that the item is accepted.
                // QuestManager is still responsible for consuming the item,
                // spawning the ingredient, and finishing the quest.
                questUI?.ShowRewardTradeFeedback(tradeResult);
                CompleteSuccessfulTrade(npc, quest, tradeResult.rewardItem);
                break;

            case RewardTradeSubmitOutcome.HaggleRequired:
                // RewardTradeManager stored the pending reward item.
                // UI should now show yellow question mark and Roll D20 button.
                OpenReturnUIForQuest(npc, quest);
                questUI?.ShowRewardTradeFeedback(tradeResult);
                break;

            case RewardTradeSubmitOutcome.Refused:
                // The item was refused.
                // No haggle attempt is consumed here.
                // Player can try another reward item.
                OpenReturnUIForQuest(npc, quest);
                questUI?.ShowRewardTradeFeedback(tradeResult);
                break;
        }
    }
    
    public void TryRollHaggle(AdventurerNPC npc)
    {
        if (npc == null)
            return;

        if (!activeQuests.TryGetValue(npc, out ActiveQuest quest) || quest == null)
            return;

        if (npc.State != AdventurerState.WaitingReward)
            return;

        if (rewardTradeManager == null)
        {
            Debug.LogError("[QUEST] RewardTradeManager is missing.");
            OpenReturnUIForQuest(npc, quest);
            return;
        }

        HaggleRollResult rollResult = rewardTradeManager.RollHaggle(npc);

        Debug.Log(
            "[QUEST HAGGLE] " +
            rollResult.message +
            " | Roll: " + rollResult.roll +
            " | Attempts left: " + rollResult.attemptsLeft
        );

        switch (rollResult.outcome)
        {
            case HaggleRollOutcome.Success:
                // Successful haggle means the adventurer accepts the pending item.
                questUI?.ShowHaggleRollFeedback(rollResult);
                CompleteSuccessfulTrade(npc, quest, rollResult.rewardItem);
                break;

            case HaggleRollOutcome.FailedCanRetry:
                // First failed roll. Player can roll one more time.
                OpenReturnUIForQuest(npc, quest);
                questUI?.ShowHaggleRollFeedback(rollResult);
                break;

            case HaggleRollOutcome.FailedNoAttemptsLeft:
                // Failed twice. Adventurer leaves without giving the ingredient.
                questUI?.ShowHaggleRollFeedback(rollResult);
                FinishQuest(npc);
                break;

            case HaggleRollOutcome.NoPendingHaggle:
                // Player tried to roll without a valid pending item.
                OpenReturnUIForQuest(npc, quest);
                questUI?.ShowHaggleRollFeedback(rollResult);
                break;
        }
    }

    public void DismissNpc(AdventurerNPC npc)
    {
        Debug.Log("[QUEST] DismissNpc: " + (npc ? npc.name : "NULL"), this);

        if (npc == null)
            return;

        // Remove all temporary quest data related to this NPC.
        activeQuests.Remove(npc);
        offerPreviews.Remove(npc);
        rewardTradeManager?.ClearSession(npc);

        questUI?.CloseAll();

        // The spawner listens to this event and removes the adventurer object.
        OnQuestFinished?.Invoke(npc);
    }

    // ------------------ Save / Load support ------------------

    public int BagIndex => bagIndex;

    public void SetBagIndexFromSave(int value)
    {
        bagIndex = Mathf.Max(0, value);
    }

    public void ClearQuestRuntimeState()
    {
        activeQuests.Clear();
        offerPreviews.Clear();
        rewardTradeManager?.ClearAllSessions();

        Debug.Log("[QUEST LOAD] Quest runtime state cleared.");
    }

    public bool TryGetActiveQuestSaveData(AdventurerNPC npc, out ActiveQuestSaveData saveData)
    {
        saveData = null;

        if (npc == null)
            return false;

        if (!activeQuests.TryGetValue(npc, out ActiveQuest quest) || quest == null)
            return false;

        float remainingReturnSeconds = 0f;

        // Do not save returnAtTime directly, because it is based on Time.time.
        // Save only the remaining time.
        if (quest.returnAtTime != float.MaxValue)
        {
            remainingReturnSeconds = Mathf.Max(0f, quest.returnAtTime - Time.time);
        }

        int haggleAttemptsLeft = rewardTradeManager != null
            ? rewardTradeManager.GetAttemptsLeft(npc)
            : RewardTradeManager.DefaultHaggleAttempts;

        saveData = new ActiveQuestSaveData
        {
            ingredientId = quest.ingredient != null ? quest.ingredient.id : "",
            amount = quest.amount,
            difficultyLevel = quest.difficultyLevel,
            remainingReturnSeconds = remainingReturnSeconds,
            haggleAttemptsLeft = haggleAttemptsLeft
        };

        Debug.Log(
            "[QUEST SAVE] Active quest for NPC: " + GetNpcDebugName(npc) +
            " | state: " + npc.State +
            " | returnAtTime: " + quest.returnAtTime +
            " | Time.time: " + Time.time +
            " | remaining: " + remainingReturnSeconds +
            " | ingredient: " + saveData.ingredientId +
            " | amount: " + saveData.amount
        );

        return true;
    }

    public void RestoreActiveQuestFromSave(
        AdventurerNPC npc,
        AdventurerState savedState,
        ActiveQuestSaveData savedQuest
    )
    {
        if (npc == null)
        {
            Debug.LogWarning("[QUEST LOAD] Cannot restore quest. NPC is NULL.");
            return;
        }

        if (savedQuest == null)
        {
            Debug.LogWarning("[QUEST LOAD] Cannot restore quest. Saved quest is NULL for NPC: " + npc.name);
            return;
        }

        IngredientData ingredient = null;

        if (itemDatabase != null && !string.IsNullOrEmpty(savedQuest.ingredientId))
        {
            ingredient = itemDatabase.GetItemById(savedQuest.ingredientId);
        }

        if (ingredient == null)
        {
            Debug.LogWarning(
                "[QUEST LOAD] Missing ingredient with id: " +
                savedQuest.ingredientId +
                ". Using default ingredient."
            );

            ingredient = defaultIngredient;
        }

        if (ingredient == null)
        {
            Debug.LogError("[QUEST LOAD] Cannot restore quest. Ingredient and defaultIngredient are NULL.");
            return;
        }

        int amount = Mathf.Max(1, savedQuest.amount);
        int difficultyLevel = Mathf.Max(1, savedQuest.difficultyLevel);
        int haggleAttemptsLeft = Mathf.Max(0, savedQuest.haggleAttemptsLeft);

        ActiveQuest quest = new ActiveQuest
        {
            npc = npc,
            ingredient = ingredient,
            amount = amount,
            difficultyLevel = difficultyLevel
        };

        if (savedState == AdventurerState.InProgress)
        {
            float remaining = Mathf.Max(0f, savedQuest.remainingReturnSeconds);

            if (remaining <= 0f)
            {
                // If remaining time is already zero, the adventurer should be treated as returned.
                quest.returnAtTime = float.MaxValue;
                npc.SetState(AdventurerState.WaitingReward);

                if (adventurerSpawner != null)
                    adventurerSpawner.NotifyAdventurerReturned(npc);
                else
                    npc.ShowAdventurer();

                Debug.Log("[QUEST LOAD] InProgress quest already finished. NPC set to WaitingReward: " + GetNpcDebugName(npc));
            }
            else
            {
                // Continue the timer from the current Time.time.
                quest.returnAtTime = Time.time + remaining;
                npc.SetState(AdventurerState.InProgress);
                npc.HideAdventurer();

                Debug.Log(
                    "[QUEST LOAD] Restored InProgress quest. NPC: " +
                    GetNpcDebugName(npc) +
                    " | remaining: " +
                    remaining
                );
            }
        }
        else if (savedState == AdventurerState.WaitingReward)
        {
            quest.returnAtTime = float.MaxValue;
            npc.SetState(AdventurerState.WaitingReward);

            // The spawner / save manager should decide queue placement.
            // As fallback, show the NPC if no spawner exists.
            if (adventurerSpawner == null)
                npc.ShowAdventurer();

            Debug.Log("[QUEST LOAD] Restored WaitingReward quest. NPC: " + GetNpcDebugName(npc));
        }
        else
        {
            quest.returnAtTime = float.MaxValue;
            npc.SetState(savedState);

            if (adventurerSpawner == null)
                npc.ShowAdventurer();

            Debug.Log(
                "[QUEST LOAD] Restored quest with state: " +
                savedState +
                " | NPC: " +
                GetNpcDebugName(npc)
            );
        }

        activeQuests[npc] = quest;
        
        if (savedState == AdventurerState.WaitingReward)
        {
            rewardTradeManager?.RestoreSessionState(npc, difficultyLevel, haggleAttemptsLeft);
        }
        else
        {
            rewardTradeManager?.ClearSession(npc);
        }

        Debug.Log(
            "[QUEST LOAD] Active quest restored for NPC: " +
            GetNpcDebugName(npc) +
            " | ingredient: " +
            ingredient.id +
            " | amount: " +
            amount +
            " | difficultyLevel: " + 
            difficultyLevel +
            " | haggleAttemptsLeft : " +
            haggleAttemptsLeft  +
            " | activeQuests count: " +
            activeQuests.Count
        );
    }

    // ------------------ Bag logic ------------------

    private QuestTarget PickTargetFromRecipes()
    {
        QuestTarget fallback = new QuestTarget
        {
            recipe = null,
            ingredient = defaultIngredient,
            amount = Mathf.Max(1, defaultAmount)
        };

        if (currentActiveRecipe == null)
        {
            Debug.LogWarning("[QUEST] No active recipe set. Using default ingredient.");
            return fallback;
        }

        if (currentActiveRecipe.ingredients == null || currentActiveRecipe.ingredients.Count == 0)
        {
            Debug.LogWarning("[QUEST] Active recipe has no ingredients: " + currentActiveRecipe.displayName);
            return fallback;
        }

        if (targetBag.Count == 0 || bagIndex >= targetBag.Count)
            RebuildAndShuffleTargetBagFromActiveRecipe();

        if (targetBag.Count == 0)
            return fallback;

        QuestTarget target = targetBag[bagIndex];
        bagIndex++;

        return target;
    }

    private void RebuildAndShuffleTargetBagFromActiveRecipe()
    {
        targetBag.Clear();
        bagIndex = 0;

        if (currentActiveRecipe == null || currentActiveRecipe.ingredients == null)
            return;

        foreach (var ingredientEntry in currentActiveRecipe.ingredients)
        {
            if (ingredientEntry == null || ingredientEntry.item == null)
                continue;

            targetBag.Add(new QuestTarget
            {
                recipe = currentActiveRecipe,
                ingredient = ingredientEntry.item,
                amount = Mathf.Max(1, ingredientEntry.amount)
            });
        }

        if (targetBag.Count == 0)
            return;

        for (int i = targetBag.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (targetBag[i], targetBag[j]) = (targetBag[j], targetBag[i]);
        }
    }

    // ------------------ Internals ------------------

    private void MarkAdventurerReturned(ActiveQuest quest)
    {
        if (quest == null || quest.npc == null)
            return;

        quest.returnAtTime = float.MaxValue;

        AkUnitySoundEngine.PostEvent("Adventurer_Returns", gameObject);

        quest.npc.SetState(AdventurerState.WaitingReward);

        // The spawner controls where the returned adventurer should appear.
        if (adventurerSpawner != null)
        {
            adventurerSpawner.NotifyAdventurerReturned(quest.npc);
        }
        else
        {
            quest.npc.ShowAdventurer();
        }

        OnAdventurerReturned?.Invoke(quest.npc);

        Debug.Log("[QUEST] Adventurer returned: " + GetNpcDebugName(quest.npc));
    }

    
    private void CompleteSuccessfulTrade(
        AdventurerNPC npc,
        ActiveQuest quest,
        InventoryItem rewardItem)
    {
        if (npc == null || quest == null)
            return;

        if (rewardItem != null)
        {
            // Notify other systems that the reward item was consumed.
            OnRewardConsumed?.Invoke(rewardItem);

            // Remove one item amount from inventory data if this item is stored in inventory.
            if (rewardItem.isInInventory && playerInventory != null)
            {
                playerInventory.RemoveItemDataAmount(rewardItem.itemData, 1);
            }

            // Destroy the physical/held reward item object.
            Destroy(rewardItem.gameObject);
        }

        // The adventurer accepted the reward.
        // Now give the promised ingredient to the player.
        houseSpawner?.SpawnObject(quest.ingredient, quest.amount);

        // Trade session is no longer needed.
        rewardTradeManager?.ClearSession(npc);

        FinishQuest(npc);
    }
    
    private void FinishQuest(AdventurerNPC npc)
    {
        if (npc == null)
            return;

        activeQuests.Remove(npc);
        offerPreviews.Remove(npc);
        rewardTradeManager?.ClearSession(npc);

        questUI?.CloseAll();

        // The spawner will remove and destroy the adventurer object.
        OnQuestFinished?.Invoke(npc);

        Debug.Log("[QUEST FINISHED] NPC: " + GetNpcDebugName(npc));
    }

    private void OpenReturnUIForQuest(AdventurerNPC npc, ActiveQuest quest)
    {
        if (npc == null || quest == null)
            return;

        int attemptsLeft = rewardTradeManager != null
            ? rewardTradeManager.GetAttemptsLeft(npc)
            : RewardTradeManager.DefaultHaggleAttempts;

        questUI?.OpenReturnUI(npc, new ReturnInfo
        {
            ingredientName = quest.ingredient != null ? quest.ingredient.displayName : "Ingredient",
            amount = quest.amount,

            difficultyLevel = quest.difficultyLevel,

            haggleAttemptsLeft = attemptsLeft
        });
    }

    private void LoadDialogueBank()
    {
        if (dialogueJsonFile == null)
        {
            bank = new QuestDialogueBankJson
            {
                intros = Array.Empty<QuestDialogueEntry>(),
                outros = Array.Empty<QuestDialogueEntry>(),
                hints = Array.Empty<QuestDialogueEntry>()
            };

            return;
        }

        bank = JsonUtility.FromJson<QuestDialogueBankJson>(dialogueJsonFile.text) ?? new QuestDialogueBankJson();

        bank.intros ??= Array.Empty<QuestDialogueEntry>();
        bank.outros ??= Array.Empty<QuestDialogueEntry>();
        bank.hints ??= Array.Empty<QuestDialogueEntry>();
    }

    private static QuestDialogueEntry PickRandom(QuestDialogueEntry[] list)
    {
        if (list == null || list.Length == 0)
            return new QuestDialogueEntry { id = "none", text = "" };

        return list[UnityEngine.Random.Range(0, list.Length)];
    }

    private static string ApplyTokens(string text, string ingredientName, int amount, string hintText)
    {
        if (string.IsNullOrEmpty(text))
            return "";

        return text
            .Replace("{ingredient}", ingredientName)
            .Replace("{amount}", amount.ToString())
            .Replace("{hint}", hintText ?? "");
    }
    

    private string GetNpcDebugName(AdventurerNPC npc)
    {
        if (npc == null)
            return "NULL";

        if (npc.Data == null)
            return npc.name + " / No AdventurerData";

        return npc.name + " / id: " + npc.Data.id + " / name: " + npc.Data.displayName;
    }
}