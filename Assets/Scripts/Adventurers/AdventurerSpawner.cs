using System;
using System.Collections.Generic;
using UnityEngine;

public class AdventurerSpawner : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private QuestManager questManager;
    [SerializeField] private QuestUI questUI;

    [Header("Database")]
    [SerializeField] private AdventurerDatabase adventurerDatabase;

    [Header("Spawn")]
    [SerializeField] private AdventurerNPC adventurerPrefab;

    [Tooltip("Fallback spawn point. Used if queuePoints is empty.")]
    [SerializeField] private Transform spawnPoint;

    [Tooltip("Queue points near the window. Index 0 is the active interaction position.")]
    [SerializeField] private List<Transform> queuePoints = new();

    [Header("Spawn Timing")]
    [SerializeField] private float minDelay = 8f;
    [SerializeField] private float maxDelay = 20f;

    [Header("Debug")]
    [SerializeField] private bool spawnOnStart = true;

    // All adventurers currently created by this spawner.
    // This includes visible queued adventurers and hidden adventurers on quests.
    private readonly List<AdventurerNPC> spawnedAdventurers = new();

    // Adventurers currently standing near the window.
    // queueAdventurers[0] is the one the player can interact with.
    private readonly List<AdventurerNPC> queueAdventurers = new();

    // Adventurers that returned from quests but cannot stand near the window yet
    // because all queue points are occupied.
    private readonly List<AdventurerNPC> pendingReturnAdventurers = new();

    private bool spawnScheduled;

    public IReadOnlyList<AdventurerNPC> SpawnedAdventurers => spawnedAdventurers;
    public IReadOnlyList<AdventurerNPC> QueueAdventurers => queueAdventurers;

    public static event Action<string> OnSpawned;
    public static event Action<string> OnReturn;

    private int QueueCapacity
    {
        get
        {
            if (queuePoints != null && queuePoints.Count > 0)
                return queuePoints.Count;

            return spawnPoint != null ? 1 : 0;
        }
    }

    private void Awake()
    {
        if (questManager == null)
            questManager = FindFirstObjectByType<QuestManager>();

        if (questUI == null)
            questUI = FindFirstObjectByType<QuestUI>();
    }

    private void OnEnable()
    {
        if (questManager != null)
        {
            // QuestManager fires this when the full quest cycle is finished
            // and the adventurer object can be removed.
            questManager.OnQuestFinished += HandleQuestFinished;
        }
    }

    private void OnDisable()
    {
        if (questManager != null)
        {
            questManager.OnQuestFinished -= HandleQuestFinished;
        }

        CancelInvoke(nameof(SpawnNow));
        spawnScheduled = false;
    }

    private void Start()
    {
        if (spawnOnStart)
            ScheduleNextSpawn();
    }

    private void ScheduleNextSpawn()
    {
        if (!HasFreeQueueSlot())
            return;

        if (spawnScheduled)
            return;

        if (!HasAvailableAdventurerData())
        {
            Debug.Log("[ADVENTURER SPAWNER] No available adventurer data to schedule.");
            return;
        }

        float delay = UnityEngine.Random.Range(minDelay, maxDelay);

        spawnScheduled = true;
        Invoke(nameof(SpawnNow), delay);

        Debug.Log("[ADVENTURER SPAWNER] Next adventurer spawn scheduled in " + delay + " seconds.");
    }

    private void SpawnNow()
    {
        spawnScheduled = false;

        // Returned adventurers should always get priority over newly spawned offered adventurers.
        TryPlacePendingReturnAdventurers();

        if (!HasFreeQueueSlot())
        {
            Debug.Log("[ADVENTURER SPAWNER] Spawn skipped. Queue is full.");
            return;
        }

        if (adventurerPrefab == null || questManager == null || questUI == null)
        {
            Debug.LogWarning("[ADVENTURER SPAWNER] Missing refs: prefab / questManager / questUI.", this);
            return;
        }

        AdventurerData data = PickRandomAvailableAdventurerData();

        if (data == null)
        {
            Debug.LogWarning("[ADVENTURER SPAWNER] No available AdventurerData found in AdventurerDatabase.", this);
            return;
        }

        Transform point = GetQueuePoint(queueAdventurers.Count);

        if (point == null)
        {
            Debug.LogWarning("[ADVENTURER SPAWNER] No valid spawn or queue point found.", this);
            return;
        }

        AdventurerNPC npc = Instantiate(adventurerPrefab, point.position, point.rotation);
        npc.Initialize(data, questManager, questUI);
        npc.SetState(AdventurerState.Offered);

        spawnedAdventurers.Add(npc);
        AddToQueue(npc, addToFront: false);

        OnSpawned?.Invoke("A new adventurer has arrived!");

        WwiseAudioManager.Instance.PostEvent("Adventurer_Arrived", npc.gameObject);

        Debug.Log(
            "[ADVENTURER SPAWNER] Spawned adventurer: " +
            data.id +
            " | Queue count: " +
            queueAdventurers.Count +
            " | Spawned count: " +
            spawnedAdventurers.Count
        );

        ScheduleNextSpawn();
    }

    private AdventurerData PickRandomAvailableAdventurerData()
    {
        if (adventurerDatabase == null ||
            adventurerDatabase.adventurers == null ||
            adventurerDatabase.adventurers.Count == 0)
        {
            return null;
        }

        List<AdventurerData> available = new List<AdventurerData>();

        foreach (AdventurerData data in adventurerDatabase.adventurers)
        {
            if (data == null)
                continue;

            if (string.IsNullOrEmpty(data.id))
            {
                Debug.LogWarning("[ADVENTURER SPAWNER] AdventurerData has empty id: " + data.name);
                continue;
            }

            // Current save/load design assumes that each active adventurer has a unique data id.
            // If we ever want duplicates of the same AdventurerData at the same time,
            // we will need a separate runtime instance id.
            if (!IsAdventurerDataAlreadySpawned(data.id))
            {
                available.Add(data);
            }
        }

        if (available.Count == 0)
            return null;

        return available[UnityEngine.Random.Range(0, available.Count)];
    }

    private bool HasAvailableAdventurerData()
    {
        if (adventurerDatabase == null ||
            adventurerDatabase.adventurers == null ||
            adventurerDatabase.adventurers.Count == 0)
        {
            return false;
        }

        foreach (AdventurerData data in adventurerDatabase.adventurers)
        {
            if (data == null)
                continue;

            if (string.IsNullOrEmpty(data.id))
                continue;

            if (!IsAdventurerDataAlreadySpawned(data.id))
                return true;
        }

        return false;
    }

    private bool IsAdventurerDataAlreadySpawned(string adventurerId)
    {
        foreach (AdventurerNPC npc in spawnedAdventurers)
        {
            if (npc == null || npc.Data == null)
                continue;

            if (npc.Data.id == adventurerId)
                return true;
        }

        return false;
    }

    private bool HasFreeQueueSlot()
    {
        return queueAdventurers.Count < QueueCapacity;
    }

    private Transform GetQueuePoint(int index)
    {
        if (queuePoints != null && queuePoints.Count > 0)
        {
            if (index >= 0 && index < queuePoints.Count)
                return queuePoints[index];

            return null;
        }

        return spawnPoint;
    }

    private bool AddToQueue(AdventurerNPC npc, bool addToFront)
    {
        if (npc == null)
            return false;

        if (queueAdventurers.Contains(npc))
            return true;

        if (!HasFreeQueueSlot())
            return false;

        if (addToFront)
            queueAdventurers.Insert(0, npc);
        else
            queueAdventurers.Add(npc);

        RebuildQueuePositions();

        return true;
    }

    private void RemoveFromQueue(AdventurerNPC npc)
    {
        if (npc == null)
            return;

        queueAdventurers.Remove(npc);
        RebuildQueuePositions();
    }

    private void RebuildQueuePositions()
    {
        for (int i = 0; i < queueAdventurers.Count; i++)
        {
            AdventurerNPC npc = queueAdventurers[i];

            if (npc == null)
                continue;

            Transform point = GetQueuePoint(i);

            if (point == null)
                continue;

            npc.transform.position = point.position;
            npc.transform.rotation = point.rotation;

            npc.ShowAdventurer();

            // Only the first adventurer in the queue should be interactable.
            // Other visible adventurers wait for their turn.
            npc.SetInteractionEnabled(i == 0);

            Debug.Log(
                "[ADVENTURER SPAWNER] Queue position updated. NPC: " +
                GetNpcDebugId(npc) +
                " | Queue index: " +
                i
            );
        }
    }

    private void TryPlacePendingReturnAdventurers()
    {
        while (pendingReturnAdventurers.Count > 0 && HasFreeQueueSlot())
        {
            AdventurerNPC npc = pendingReturnAdventurers[0];
            pendingReturnAdventurers.RemoveAt(0);

            if (npc == null)
                continue;

            // Returned adventurers should have priority over newly spawned offered adventurers.
            bool added = AddToQueue(npc, addToFront: true);

            if (!added)
            {
                pendingReturnAdventurers.Insert(0, npc);
                break;
            }

            OnReturn?.Invoke("An adventurer has returned from a quest!");
        }
    }

    // Called by QuestManager when the player accepts a quest.
    // The adventurer leaves the queue and becomes hidden while the quest timer runs.
    public void NotifyQuestAccepted(AdventurerNPC npc)
    {
        if (npc == null)
            return;

        RemoveFromQueue(npc);

        npc.SetState(AdventurerState.InProgress);
        npc.HideAdventurer();

        Debug.Log("[ADVENTURER SPAWNER] Adventurer accepted quest and left queue: " + GetNpcDebugId(npc));

        TryPlacePendingReturnAdventurers();
        RebuildQueuePositions();
        ScheduleNextSpawn();
    }

    // Called by QuestManager when the adventurer return timer finishes.
    // If the queue has space, the adventurer appears near the window.
    // If the queue is full, the adventurer waits hidden in pendingReturnAdventurers.
    public void NotifyAdventurerReturned(AdventurerNPC npc)
    {
        if (npc == null)
            return;

        if (!spawnedAdventurers.Contains(npc))
            spawnedAdventurers.Add(npc);

        RemoveFromQueue(npc);

        npc.SetState(AdventurerState.WaitingReward);

        if (HasFreeQueueSlot())
        {
            AddToQueue(npc, addToFront: true);
            Debug.Log("[ADVENTURER SPAWNER] Returned adventurer added to queue: " + GetNpcDebugId(npc));
        }
        else
        {
            npc.HideAdventurer();

            if (!pendingReturnAdventurers.Contains(npc))
                pendingReturnAdventurers.Add(npc);

            Debug.Log("[ADVENTURER SPAWNER] Returned adventurer is waiting for queue space: " + GetNpcDebugId(npc));
        }

        RebuildQueuePositions();
    }

    private void HandleQuestFinished(AdventurerNPC npc)
    {
        if (npc == null)
            return;

        RemoveFromQueue(npc);
        pendingReturnAdventurers.Remove(npc);
        spawnedAdventurers.Remove(npc);

        Debug.Log("[ADVENTURER SPAWNER] Quest fully finished. Removing adventurer: " + GetNpcDebugId(npc));

        Destroy(npc.gameObject);

        TryPlacePendingReturnAdventurers();
        RebuildQueuePositions();
        ScheduleNextSpawn();
    }

    public int GetQueueIndex(AdventurerNPC npc)
    {
        if (npc == null)
            return -1;

        return queueAdventurers.IndexOf(npc);
    }

    public AdventurerData GetAdventurerDataById(string adventurerId)
    {
        if (string.IsNullOrEmpty(adventurerId))
            return null;

        if (adventurerDatabase == null ||
            adventurerDatabase.adventurers == null ||
            adventurerDatabase.adventurers.Count == 0)
        {
            Debug.LogWarning("[ADVENTURER SPAWNER] AdventurerDatabase is missing or empty.");
            return null;
        }

        foreach (AdventurerData data in adventurerDatabase.adventurers)
        {
            if (data == null)
                continue;

            if (data.id == adventurerId)
                return data;
        }

        return null;
    }

    // Used by save/load.
    public AdventurerNPC SpawnAdventurerFromSave(string adventurerId, AdventurerState state, int queueIndex)
    {
        AdventurerData data = GetAdventurerDataById(adventurerId);

        if (data == null)
        {
            Debug.LogWarning("[ADVENTURER SPAWNER] Cannot restore adventurer. Missing data id: " + adventurerId);
            return null;
        }

        Transform point = GetRestoreSpawnPoint(queueIndex);

        if (point == null)
        {
            Debug.LogWarning("[ADVENTURER SPAWNER] Cannot restore adventurer. Missing spawn point.");
            return null;
        }

        AdventurerNPC npc = Instantiate(adventurerPrefab, point.position, point.rotation);
        npc.Initialize(data, questManager, questUI);
        npc.SetState(state);

        spawnedAdventurers.Add(npc);

        if (state == AdventurerState.InProgress)
        {
            npc.HideAdventurer();
        }
        else if (queueIndex >= 0)
        {
            InsertIntoQueueAt(npc, queueIndex);
        }
        else if (state == AdventurerState.WaitingReward)
        {
            npc.HideAdventurer();

            if (!pendingReturnAdventurers.Contains(npc))
                pendingReturnAdventurers.Add(npc);
        }
        else
        {
            if (HasFreeQueueSlot())
                AddToQueue(npc, addToFront: false);
            else
                npc.HideAdventurer();
        }

        Debug.Log(
            "[ADVENTURER SPAWNER] Restored adventurer from save: " +
            adventurerId +
            " | State: " +
            state +
            " | Queue index: " +
            queueIndex
        );

        return npc;
    }

    private Transform GetRestoreSpawnPoint(int queueIndex)
    {
        if (queueIndex >= 0 && QueueCapacity > 0)
        {
            int clampedIndex = Mathf.Clamp(queueIndex, 0, QueueCapacity - 1);
            Transform queuePoint = GetQueuePoint(clampedIndex);

            if (queuePoint != null)
                return queuePoint;
        }

        return spawnPoint;
    }

    private void InsertIntoQueueAt(AdventurerNPC npc, int queueIndex)
    {
        if (npc == null)
            return;

        if (queueAdventurers.Contains(npc))
            return;

        if (!HasFreeQueueSlot())
        {
            npc.HideAdventurer();

            if (npc.State == AdventurerState.WaitingReward && !pendingReturnAdventurers.Contains(npc))
                pendingReturnAdventurers.Add(npc);

            return;
        }

        int index = Mathf.Clamp(queueIndex, 0, queueAdventurers.Count);
        queueAdventurers.Insert(index, npc);

        RebuildQueuePositions();
    }

    // Used before loading save data.
    public void ClearAllAdventurersForLoad()
    {
        CancelInvoke(nameof(SpawnNow));
        spawnScheduled = false;

        foreach (AdventurerNPC npc in spawnedAdventurers)
        {
            if (npc != null)
                Destroy(npc.gameObject);
        }

        spawnedAdventurers.Clear();
        queueAdventurers.Clear();
        pendingReturnAdventurers.Clear();

        Debug.Log("[ADVENTURER SPAWNER] Cleared all spawned adventurers for load.");
    }

    private string GetNpcDebugId(AdventurerNPC npc)
    {
        if (npc == null)
            return "NULL";

        if (npc.Data == null)
            return npc.name + " / No AdventurerData";

        return npc.Data.id;
    }
}