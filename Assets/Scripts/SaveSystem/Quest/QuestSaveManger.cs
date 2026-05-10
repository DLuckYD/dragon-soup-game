using System.Collections.Generic;
using UnityEngine;

public class QuestSaveManager : MonoBehaviour
{
    public static QuestSaveManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private QuestManager questManager;
    [SerializeField] private AdventurerSpawner adventurerSpawner;

    private void Awake()
    {
        Instance = this;

        if (questManager == null)
            questManager = FindFirstObjectByType<QuestManager>();

        if (adventurerSpawner == null)
            adventurerSpawner = FindFirstObjectByType<AdventurerSpawner>();
    }

    public QuestSaveData CaptureSaveData()
    {
        Debug.Log("========== QUEST SAVE START ==========");

        QuestSaveData saveData = new QuestSaveData();

        if (questManager == null)
        {
            Debug.LogWarning("[QUEST SAVE] QuestManager is NULL.");
            return saveData;
        }

        if (adventurerSpawner == null)
        {
            Debug.LogWarning("[QUEST SAVE] AdventurerSpawner is NULL.");
            return saveData;
        }

        saveData.bagIndex = questManager.BagIndex;

        IReadOnlyList<AdventurerNPC> adventurers = adventurerSpawner.SpawnedAdventurers;

        Debug.Log("[QUEST SAVE] Spawned adventurers count: " + adventurers.Count);

        foreach (AdventurerNPC npc in adventurers)
        {
            if (npc == null)
            {
                Debug.LogWarning("[QUEST SAVE] Found NULL adventurer. Skipping.");
                continue;
            }

            if (npc.Data == null)
            {
                Debug.LogWarning("[QUEST SAVE] Adventurer has NULL data: " + npc.name);
                continue;
            }

            if (string.IsNullOrEmpty(npc.Data.id))
            {
                Debug.LogWarning("[QUEST SAVE] Adventurer has EMPTY id: " + npc.name);
                continue;
            }

            int queueIndex = adventurerSpawner.GetQueueIndex(npc);

            AdventurerQuestSaveData npcSave = new AdventurerQuestSaveData
            {
                adventurerId = npc.Data.id,
                state = npc.State.ToString(),
                queueIndex = queueIndex,
                hasActiveQuest = false,
                activeQuest = null
            };

            if (questManager.TryGetActiveQuestSaveData(npc, out ActiveQuestSaveData activeQuestSave))
            {
                npcSave.hasActiveQuest = true;
                npcSave.activeQuest = activeQuestSave;

                Debug.Log(
                    "[QUEST SAVE] Saved active quest for adventurer: " +
                    npc.Data.id +
                    " | state: " +
                    npc.State +
                    " | queueIndex: " +
                    queueIndex +
                    " | remaining: " +
                    activeQuestSave.remainingReturnSeconds
                );
            }
            else
            {
                Debug.Log(
                    "[QUEST SAVE] Saved adventurer without active quest: " +
                    npc.Data.id +
                    " | state: " +
                    npc.State +
                    " | queueIndex: " +
                    queueIndex
                );
            }

            saveData.adventurers.Add(npcSave);
        }

        Debug.Log("[QUEST SAVE] Final saved adventurers count: " + saveData.adventurers.Count);
        Debug.Log("========== QUEST SAVE END ==========");

        return saveData;
    }

    public void RestoreSaveData(QuestSaveData saveData)
    {
        Debug.Log("========== QUEST LOAD START ==========");

        if (saveData == null || saveData.adventurers == null)
        {
            Debug.LogWarning("[QUEST LOAD] Save data is NULL.");
            return;
        }

        if (questManager == null)
        {
            Debug.LogWarning("[QUEST LOAD] QuestManager is NULL.");
            return;
        }

        if (adventurerSpawner == null)
        {
            Debug.LogWarning("[QUEST LOAD] AdventurerSpawner is NULL.");
            return;
        }

        // Clear runtime quest dictionaries.
        questManager.ClearQuestRuntimeState();

        // Restore quest generation progress.
        questManager.SetBagIndexFromSave(saveData.bagIndex);

        // Remove all current adventurers before restoring saved ones.
        // This prevents duplicates after loading.
        adventurerSpawner.ClearAllAdventurersForLoad();

        Debug.Log("[QUEST LOAD] Saved adventurers count: " + saveData.adventurers.Count);

        foreach (AdventurerQuestSaveData savedNpc in saveData.adventurers)
        {
            if (savedNpc == null)
            {
                Debug.LogWarning("[QUEST LOAD] Saved adventurer entry is NULL. Skipping.");
                continue;
            }

            if (string.IsNullOrEmpty(savedNpc.adventurerId))
            {
                Debug.LogWarning("[QUEST LOAD] Saved adventurer has EMPTY id. Skipping.");
                continue;
            }

            if (!TryParseState(savedNpc.state, out AdventurerState restoredState))
            {
                Debug.LogWarning(
                    "[QUEST LOAD] Cannot parse adventurer state: " +
                    savedNpc.state +
                    ". Using Offered."
                );

                restoredState = AdventurerState.Offered;
            }

            Debug.Log(
                "[QUEST LOAD] Restoring adventurer: " +
                savedNpc.adventurerId +
                " | state: " +
                restoredState +
                " | queueIndex: " +
                savedNpc.queueIndex +
                " | hasActiveQuest: " +
                savedNpc.hasActiveQuest
            );

            AdventurerNPC npc = adventurerSpawner.SpawnAdventurerFromSave(
                savedNpc.adventurerId,
                restoredState,
                savedNpc.queueIndex
            );

            if (npc == null)
            {
                Debug.LogWarning(
                    "[QUEST LOAD] Failed to spawn adventurer from save. Id: " +
                    savedNpc.adventurerId
                );

                continue;
            }

            if (savedNpc.hasActiveQuest && savedNpc.activeQuest != null)
            {
                questManager.RestoreActiveQuestFromSave(
                    npc,
                    restoredState,
                    savedNpc.activeQuest
                );
            }
            else
            {
                RestoreAdventurerWithoutActiveQuest(npc, restoredState, savedNpc.adventurerId);
            }
        }

        Debug.Log("[QUEST LOAD] Restored adventurers: " + saveData.adventurers.Count);
        Debug.Log("========== QUEST LOAD END ==========");
    }

    private bool TryParseState(string stateText, out AdventurerState state)
    {
        return System.Enum.TryParse(stateText, out state);
    }

    private void RestoreAdventurerWithoutActiveQuest(
        AdventurerNPC npc,
        AdventurerState savedState,
        string adventurerId
    )
    {
        if (npc == null)
            return;

        // Adventurer has no active quest and is simply waiting for a quest.
        if (savedState == AdventurerState.Offered)
        {
            npc.SetState(AdventurerState.Offered);

            Debug.Log(
                "[QUEST LOAD] Restored adventurer without active quest as Offered: " +
                adventurerId
            );

            return;
        }

        // Safety fallback.
        // If an adventurer was saved as InProgress or WaitingReward but has no quest data,
        // this state is invalid. We reset it to Offered.
        Debug.LogWarning(
            "[QUEST LOAD] Adventurer has state " +
            savedState +
            " but no active quest. Resetting to Offered. Adventurer id: " +
            adventurerId
        );

        npc.SetState(AdventurerState.Offered);
    }
}