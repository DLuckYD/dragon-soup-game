using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private QuestUI questUI;
    [SerializeField] private HouseSpawner houseSpawner;
    [SerializeField] private HotbarManager playerInventory;

    [Header("Dialogue JSON")]
    [SerializeField] private TextAsset dialogueJsonFile;

    [Header("Quest Generation (MVP)")]
    [SerializeField] private ItemData defaultIngredient;
    [SerializeField] private int defaultAmount = 3;
    [SerializeField] private int defaultMinRewardValue = 10;
    [SerializeField] private float defaultReturnDelaySeconds = 15f;

    [Header("Recipe Pool")]
    [SerializeField] private List<Recipe> recipePool = new();

    public event Action<object> OnRewardConsumed;
    public event Action<AdventurerNPC> OnQuestFinished;

    private QuestDialogueBankJson bank;

    private readonly Dictionary<AdventurerNPC, ActiveQuest> activeQuests = new();
    private readonly Dictionary<AdventurerNPC, OfferPreview> offerPreviews = new();

    // ------------------ Bag (no repeats until exhausted) ------------------
    private readonly List<QuestTarget> targetBag = new();
    private int bagIndex = 0;

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
        public ItemData ingredientData;
        public string ingredientName;
        public Sprite ingredientIcon;
        public int amount;

        public int minRewardValue;
        public float returnDelaySeconds;
    }

    private struct QuestTarget
    {
        public Recipe recipe;
        public ItemData ingredient;
        public int amount;
    }

    private class ActiveQuest
    {
        public AdventurerNPC npc;

        public ItemData ingredient;
        public int amount;
        public int minRewardValue;
        public float returnAtTime;

        public int attemptsLeft;

        public string introId;
        public string outroId;
        public string hintId;
        public string hintText;
    }

    public struct ReturnInfo
    {
        public string ingredientName;
        public int amount;
        public int minRewardValue;
        public int attemptsLeft;
    }

    // ------------------ Unity ------------------

    private void Awake()
    {
        if (questUI != null)
            questUI.Initialize(this);

        LoadDialogueBank();
    }

    private void Update()
    {
        foreach (var kv in activeQuests)
        {
            var quest = kv.Value;

            // Adventurer returns once
            if (quest != null && quest.returnAtTime != float.MaxValue && Time.time >= quest.returnAtTime)
            {
                quest.returnAtTime = float.MaxValue;
                AkUnitySoundEngine.PostEvent("Adventurer_Returns", gameObject);
                quest.npc.SetState(AdventurerState.WaitingReward);
                quest.npc.ShowAdventurer();

                // Return UI НЕ открываем автоматически (только по T)
            }
        }
    }

    // ------------------ Public API ------------------

    public bool TryOpenReturnUI(AdventurerNPC npc)
    {
        if (npc == null || questUI == null) return false;

        if (!activeQuests.TryGetValue(npc, out var quest) || quest == null)
            return false;

        if (npc.State != AdventurerState.WaitingReward)
            return false;

        var info = new ReturnInfo
        {
            ingredientName = quest.ingredient != null ? quest.ingredient.displayName : "Ingredient",
            amount = quest.amount,
            minRewardValue = quest.minRewardValue,
            attemptsLeft = quest.attemptsLeft
        };

        questUI.OpenReturnUI(npc, info);
        return true;
    }

    public OfferPreview GetOrCreateOfferPreview(AdventurerNPC npc)
    {
        if (npc == null) return default;

        if (offerPreviews.TryGetValue(npc, out var cached))
            return cached;

        // 1) Берём цель из "колоды" без повторов (ingredient + amount из рецепта)
        var target = PickTargetFromRecipes();

        ItemData ingredient = target.ingredient != null ? target.ingredient : defaultIngredient;
        int amount = Mathf.Max(1, target.amount);

        if (ingredient == null)
            ingredient = defaultIngredient;

        if (amount <= 0)
            amount = Mathf.Max(1, defaultAmount);

        int minValue = Mathf.Max(0, defaultMinRewardValue);
        float delay = Mathf.Max(1f, defaultReturnDelaySeconds);

        // 2) Диалоги
        var intro = PickRandom(bank?.intros);
        var outro = PickRandom(bank?.outros);
        var hint = PickRandom(bank?.hints);

        string ingredientName = ingredient != null ? ingredient.displayName : "Ingredient";
        Sprite ingredientIcon = ingredient != null ? ingredient.icon : null;
        string hintText = hint.text ?? "";

        string introText = ApplyTokens(intro.text ?? "", ingredientName, amount, hintText);

        string outroRaw = (outro.text ?? "");
        string outroText = ApplyTokens(outroRaw, ingredientName, amount, hintText);

        // Если в outro нет {hint}, добавим hint отдельной строкой
        if (!outroRaw.Contains("{hint}", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(hintText))
            outroText = $"Hint: {hintText}\n{outroText}";

        var preview = new OfferPreview
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

            minRewardValue = minValue,
            returnDelaySeconds = delay
        };

        offerPreviews[npc] = preview;
        return preview;
    }

    public void AcceptQuest(AdventurerNPC npc)
    {
        if (npc == null) return;
        if (activeQuests.ContainsKey(npc)) return;

        var preview = GetOrCreateOfferPreview(npc);

        var quest = new ActiveQuest
        {
            npc = npc,
            ingredient = preview.ingredientData != null ? preview.ingredientData : defaultIngredient,
            amount = preview.amount,
            minRewardValue = preview.minRewardValue,
            returnAtTime = Time.time + preview.returnDelaySeconds,
            attemptsLeft = 2,

            introId = preview.introId,
            outroId = preview.outroId,
            hintId = preview.hintId,
            hintText = preview.hintText
        };

        activeQuests[npc] = quest;
        offerPreviews.Remove(npc);

        npc.SetState(AdventurerState.InProgress);
        npc.HideAdventurer();
    }

    public void RejectReturnedAdventurer(AdventurerNPC npc)
    {
        if (npc == null) return;

        if (!activeQuests.TryGetValue(npc, out var quest) || quest == null)
            return;

        if (npc.State != AdventurerState.WaitingReward)
            return;

        FinishQuest(npc);
    }

    public void TrySubmitReward(AdventurerNPC npc, PlayerInteraction playerInteraction)
    {
        if (npc == null) return;

        if (!activeQuests.TryGetValue(npc, out var quest) || quest == null)
            return;

        if (npc.State != AdventurerState.WaitingReward)
            return;

        if (playerInteraction == null || playerInteraction.getHeldItem() == null)
        {
            questUI?.OpenReturnUI(npc, new ReturnInfo
            {
                ingredientName = quest.ingredient != null ? quest.ingredient.displayName : "Ingredient",
                amount = quest.amount,
                minRewardValue = quest.minRewardValue,
                attemptsLeft = quest.attemptsLeft
            });
            return;
        }

        if (!TryGetItemValue(playerInteraction.getHeldItem(), out int value))
        {
            questUI?.OpenReturnUI(npc, new ReturnInfo
            {
                ingredientName = quest.ingredient != null ? quest.ingredient.displayName : "Ingredient",
                amount = quest.amount,
                minRewardValue = quest.minRewardValue,
                attemptsLeft = quest.attemptsLeft
            });
            return;
        }

        quest.attemptsLeft--;

        if (value >= quest.minRewardValue)
        {
            // SUCCESS
            if (playerInteraction.getHeldItem().isInInventory)
            {
                playerInventory.RemoveItemDataAmount(playerInteraction.getHeldItem().itemData, 1);
            }

            Destroy(playerInteraction.getHeldItem().gameObject);

            // Spawn ingredients in the house
            houseSpawner?.SpawnObject(quest.ingredient, quest.amount);

            FinishQuest(npc);
        }
        else
        {
            if (quest.attemptsLeft <= 0)
            {
                FinishQuest(npc);
            }
            else
            {
                questUI?.OpenReturnUI(npc, new ReturnInfo
                {
                    ingredientName = quest.ingredient != null ? quest.ingredient.displayName : "Ingredient",
                    amount = quest.amount,
                    minRewardValue = quest.minRewardValue,
                    attemptsLeft = quest.attemptsLeft
                });
            }
        }
    }

    // ------------------ Bag logic (NO repeats until all used) ------------------

    private QuestTarget PickTargetFromRecipes()
    {
        var fallback = new QuestTarget
        {
            recipe = null,
            ingredient = defaultIngredient,
            amount = Mathf.Max(1, defaultAmount)
        };

        if (recipePool == null || recipePool.Count == 0)
            return fallback;

        // Build/shuffle bag when empty or exhausted
        if (targetBag.Count == 0 || bagIndex >= targetBag.Count)
            RebuildAndShuffleTargetBag();

        if (targetBag.Count == 0)
            return fallback;

        var t = targetBag[bagIndex];
        bagIndex++;
        return t;
    }

    private void RebuildAndShuffleTargetBag()
    {
        targetBag.Clear();
        bagIndex = 0;

        // Unique by ingredient (ItemData). One ingredient appears once in the bag.
        var usedIngredients = new HashSet<ItemData>();

        foreach (var recipe in recipePool)
        {
            if (recipe == null || recipe.ingredients == null) continue;

            foreach (var ing in recipe.ingredients)
            {
                if (ing == null || ing.item == null) continue;

                if (!usedIngredients.Add(ing.item))
                    continue;

                targetBag.Add(new QuestTarget
                {
                    recipe = recipe,
                    ingredient = ing.item,
                    amount = Mathf.Max(1, ing.amount)
                });
            }
        }

        if (targetBag.Count == 0)
            return;

        // Shuffle (Fisher–Yates)
        for (int i = targetBag.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (targetBag[i], targetBag[j]) = (targetBag[j], targetBag[i]);
        }
    }

    // ------------------ Internals ------------------

    private void FinishQuest(AdventurerNPC npc)
    {
        if (npc == null) return;

        npc.SetState(AdventurerState.Offered);
        npc.HideAdventurer();

        activeQuests.Remove(npc);
        offerPreviews.Remove(npc);

        questUI?.CloseAll();
        OnQuestFinished?.Invoke(npc);
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
        if (string.IsNullOrEmpty(text)) return "";

        return text
            .Replace("{ingredient}", ingredientName)
            .Replace("{amount}", amount.ToString())
            .Replace("{hint}", hintText ?? "");
    }

    private bool TryGetItemValue(object item, out int value)
    {
        value = 0;
        if (item == null) return false;

        var t = item.GetType();

        var prop = t.GetProperty("value", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
               ?? t.GetProperty("Value", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

        if (prop != null && prop.PropertyType == typeof(int))
        {
            value = (int)prop.GetValue(item);
            return true;
        }

        var field = t.GetField("value", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
                ?? t.GetField("Value", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

        if (field != null && field.FieldType == typeof(int))
        {
            value = (int)field.GetValue(item);
            return true;
        }

        return false;
    }

    public void DismissNpc(AdventurerNPC npc)
    {
        Debug.Log($"[QuestManager] DismissNpc: {(npc ? npc.name : "NULL")}", this);

        if (npc == null) return;

        // прибрати все що могло бути повʼязане з цим NPC
        activeQuests.Remove(npc);
        offerPreviews.Remove(npc);

        questUI?.CloseAll();

        // сховати NPC
        npc.HideAdventurer();

        // повідомити спавнер, щоб він видалив NPC і заспавнив наступного
        OnQuestFinished?.Invoke(npc);
    }


}
