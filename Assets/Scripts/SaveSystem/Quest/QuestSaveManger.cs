//using UnityEngine;

//using System.Collections.Generic;
//using UnityEngine;

//public class QuestSaveManager : MonoBehaviour
//{
//    public static QuestSaveManager Instance { get; private set; }

//    [SerializeField] private QuestManager questManager;

//    private void Awake()
//    {
//        Instance = this;

//        if (questManager == null)
//            questManager = FindFirstObjectByType<QuestManager>();
//    }

//    public QuestSaveData CaptureSaveData()
//    {
//        QuestSaveData saveData = new QuestSaveData();

//        if (questManager == null)
//        {
//            Debug.LogWarning("[QUEST SAVE] QuestManager is NULL.");
//            return saveData;
//        }

//        saveData.bagIndex = questManager.BagIndex;

//        AdventurerNPC[] npcs = FindObjectsByType<AdventurerNPC>(
//            FindObjectsInactive.Include,
//            FindObjectsSortMode.None
//        );

//        Debug.Log("[QUEST SAVE] Found AdventurerNPC count: " + npcs.Length);

//        foreach (AdventurerNPC npc in npcs)
//        {
//            if (npc == null)
//            {
//                Debug.LogWarning("[QUEST SAVE] Found NULL npc.");
//                continue;
//            }

//            if (npc.Data == null)
//            {
//                Debug.LogWarning("[QUEST SAVE] NPC has NULL Data: " + npc.name);
//                continue;
//            }

//            Debug.Log(
//                "[QUEST SAVE] Checking NPC: " + npc.name +
//                " | adventurerId: " + npc.Data.id +
//                " | displayName: " + npc.Data.displayName +
//                " | state: " + npc.State +
//                " | activeInHierarchy: " + npc.gameObject.activeInHierarchy
//            );

//            AdventurerQuestSaveData npcSave = new AdventurerQuestSaveData
//            {
//                adventurerId = npc.Data.id,
//                state = npc.State.ToString(),
//                hasActiveQuest = false,
//                activeQuest = null
//            };

//            if (questManager.TryGetActiveQuestSaveData(npc, out ActiveQuestSaveData activeQuestSave))
//            {
//                npcSave.hasActiveQuest = true;
//                npcSave.activeQuest = activeQuestSave;

//                Debug.Log("[QUEST SAVE] NPC has active quest: " + npc.Data.id);
//            }
//            else
//            {
//                Debug.Log("[QUEST SAVE] NPC has NO active quest: " + npc.Data.id);
//            }

//            saveData.adventurers.Add(npcSave);
//        }

//        Debug.Log("[QUEST SAVE] Final saved adventurers count: " + saveData.adventurers.Count);

//        return saveData;
//    }

//    public void RestoreSaveData(QuestSaveData saveData)
//    {
//        Debug.Log("========== QUEST LOAD START ==========");

//        if (saveData == null || saveData.adventurers == null)
//        {
//            Debug.LogWarning("[QUEST LOAD] Save data is NULL.");
//            return;
//        }

//        if (questManager == null)
//        {
//            Debug.LogWarning("[QUEST LOAD] QuestManager is NULL.");
//            return;
//        }

//        questManager.ClearQuestRuntimeState();
//        questManager.SetBagIndexFromSave(saveData.bagIndex);

//        AdventurerNPC[] npcs = FindObjectsByType<AdventurerNPC>(
//            FindObjectsInactive.Include,
//            FindObjectsSortMode.None
//        );

//        Debug.Log("[QUEST LOAD] Found AdventurerNPC objects: " + npcs.Length);

//        Dictionary<string, AdventurerNPC> npcsById = new Dictionary<string, AdventurerNPC>();

//        foreach (AdventurerNPC npc in npcs)
//        {
//            if (npc == null)
//                continue;

//            if (npc.Data == null)
//            {
//                Debug.LogWarning("[QUEST LOAD] NPC has NULL AdventurerData: " + npc.name);
//                continue;
//            }

//            if (string.IsNullOrEmpty(npc.Data.id))
//            {
//                Debug.LogWarning("[QUEST LOAD] NPC has EMPTY AdventurerData id: " + npc.name);
//                continue;
//            }

//            if (npcsById.ContainsKey(npc.Data.id))
//            {
//                Debug.LogWarning(
//                    "[QUEST LOAD] Duplicate adventurer id found: " +
//                    npc.Data.id +
//                    " on NPC: " +
//                    npc.name
//                );
//                continue;
//            }

//            npcsById.Add(npc.Data.id, npc);

//            Debug.Log(
//                "[QUEST LOAD] Registered NPC: " +
//                npc.name +
//                " | id: " +
//                npc.Data.id
//            );
//        }

//        Debug.Log("[QUEST LOAD] NPCs registered by id: " + npcsById.Count);
//        Debug.Log("[QUEST LOAD] Saved adventurers count: " + saveData.adventurers.Count);

//        foreach (AdventurerQuestSaveData savedNpc in saveData.adventurers)
//        {
//            if (savedNpc == null)
//            {
//                Debug.LogWarning("[QUEST LOAD] Saved NPC entry is NULL. Skipping.");
//                continue;
//            }

//            if (string.IsNullOrEmpty(savedNpc.adventurerId))
//            {
//                Debug.LogWarning("[QUEST LOAD] Saved adventurer has EMPTY id. Skipping.");
//                continue;
//            }

//            if (!npcsById.TryGetValue(savedNpc.adventurerId, out AdventurerNPC npc))
//            {
//                Debug.LogWarning(
//                    "[QUEST LOAD] Could not find NPC with id: " +
//                    savedNpc.adventurerId
//                );
//                continue;
//            }

//            //AdventurerState savedState = savedNpc.state;

//            //Debug.Log(
//            //    "[QUEST LOAD] Restoring adventurer: " +
//            //    savedNpc.adventurerId +
//            //    " | savedState: " +
//            //    savedState +
//            //    " | hasActiveQuest: " +
//            //    savedNpc.hasActiveQuest
//            //);

//            //if (savedNpc.hasActiveQuest && savedNpc.activeQuest != null)
//            //{
//            //    questManager.RestoreActiveQuestFromSave(
//            //        npc,
//            //        savedState,
//            //        savedNpc.activeQuest
//            //    );

//            //    continue;
//            //}

//            //RestoreAdventurerWithoutActiveQuest(npc, savedState, savedNpc.adventurerId);
//        }

//        Debug.Log("[QUEST LOAD] Restored adventurers: " + saveData.adventurers.Count);
//        Debug.Log("========== QUEST LOAD END ==========");
//    }

//    private void RestoreAdventurerWithoutActiveQuest(
//    AdventurerNPC npc,
//    AdventurerState savedState,
//    string adventurerId
//)
//    {
//        if (npc == null)
//            return;

//        if (savedState == AdventurerState.Offered)
//        {
//            npc.SetState(AdventurerState.Offered);
//            npc.ShowAdventurer();

//            Debug.Log(
//                "[QUEST LOAD] Restored adventurer without active quest as Offered: " +
//                adventurerId
//            );

//            return;
//        }

//        Debug.LogWarning(
//            "[QUEST LOAD] Adventurer has state " +
//            savedState +
//            " but no active quest. Resetting to Offered. Adventurer id: " +
//            adventurerId
//        );

//        npc.SetState(AdventurerState.Offered);
//        npc.ShowAdventurer();
//    }
//}