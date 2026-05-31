using System;
using System.Collections.Generic;
using UnityEngine;
public class AdventurerSpawner : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private QuestManager questManager;
    [SerializeField] private QuestUI questUI;

    [Header("Spawn")]
    [SerializeField] private AdventurerNPC adventurerPrefab;

    [Tooltip("Fallback spawn point. Used if queuePoints is empty.")]
    [SerializeField] private Transform spawnPoint;

    [Tooltip("Queue points near the window. Index 0 is the active interaction position.")]
    [SerializeField] private List<Transform> queuePoints = new();

    [SerializeField] private List<AdventurerData> adventurerPool = new();

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
            // This event already exists in your current QuestManager.
            // It should be fired when the adventurer quest cycle is fully finished.
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
        // Do not schedule a new spawn if all queue points are already occupied.
        if (!HasFreeQueueSlot())
            return;

        // Avoid multiple Invoke calls stacking on top of each other.
        if (spawnScheduled)
            return;

        float delay = UnityEngine.Random.Range(minDelay, maxDelay);

        spawnScheduled = true;
        Invoke(nameof(SpawnNow), delay);

        Debug.Log("[ADVENTURER SPAWNER] Next adventurer spawn scheduled in " + delay + " seconds.");
    }

    private void SpawnNow()
    {
        spawnScheduled = false;

        // Before spawning a new offered adventurer,
        // returned adventurers should get priority in the queue.
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
            Debug.LogWarning("[ADVENTURER SPAWNER] No available AdventurerData found in pool.", this);
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

        AkUnitySoundEngine.PostEvent("Adventurer_Arrives", gameObject);

        Debug.Log(
            "[ADVENTURER SPAWNER] Spawned adventurer: " +
            data.id +
            " | Queue count: " +
            queueAdventurers.Count +
            " | Spawned count: " +
            spawnedAdventurers.Count
        );

        // If there is still free space in the queue, schedule another adventurer.
        ScheduleNextSpawn();
    }

    private AdventurerData PickRandomAvailableAdventurerData()
    {
        if (adventurerPool == null || adventurerPool.Count == 0)
            return null;

        List<AdventurerData> available = new List<AdventurerData>();

        foreach (AdventurerData data in adventurerPool)
        {
            if (data == null)
                continue;

            if (string.IsNullOrEmpty(data.id))
            {
                Debug.LogWarning("[ADVENTURER SPAWNER] AdventurerData has empty id: " + data.name);
                continue;
            }

            // For the current save/load design, every active adventurer should have a unique id.
            // If you later want duplicates of the same AdventurerData,
            // we will need to add a runtime instance id.
            if (!IsAdventurerDataAlreadySpawned(data.id))
            {
                available.Add(data);
            }
        }

        if (available.Count == 0)
            return null;

        return available[UnityEngine.Random.Range(0, available.Count)];
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
            // The second adventurer is visible but waits for their turn.
            npc.SetInteractionEnabled(i == 0);

            Debug.Log(
                "[ADVENTURER SPAWNER] Queue position updated. NPC: " +
                npc.Data.id +
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

    // This method should be called by QuestManager when the player accepts a quest.
    // The adventurer leaves the window queue and becomes hidden while the quest timer runs.
    public void NotifyQuestAccepted(AdventurerNPC npc)
    {
        if (npc == null)
            return;

        RemoveFromQueue(npc);

        npc.SetState(AdventurerState.InProgress);
        npc.HideAdventurer();

        Debug.Log("[ADVENTURER SPAWNER] Adventurer accepted quest and left queue: " + npc.Data.id);

        TryPlacePendingReturnAdventurers();
        RebuildQueuePositions();
        ScheduleNextSpawn();
    }

    // This method should be called by QuestManager when the adventurer return timer finishes.
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

            OnReturn?.Invoke("An adventurer has returned from a quest!");

            Debug.Log("[ADVENTURER SPAWNER] Returned adventurer added to queue: " + npc.Data.id);
        }
        else
        {
            npc.HideAdventurer();

            if (!pendingReturnAdventurers.Contains(npc))
                pendingReturnAdventurers.Add(npc);

            Debug.Log("[ADVENTURER SPAWNER] Returned adventurer is waiting for queue space: " + npc.Data.id);
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

        Debug.Log("[ADVENTURER SPAWNER] Quest fully finished. Removing adventurer: " + npc.Data.id);

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

        foreach (AdventurerData data in adventurerPool)
        {
            if (data == null)
                continue;

            if (data.id == adventurerId)
                return data;
        }

        return null;
    }

    // This will be useful later for save/load.
    public AdventurerNPC SpawnAdventurerFromSave(string adventurerId, AdventurerState state, int queueIndex)
    {
        AdventurerData data = GetAdventurerDataById(adventurerId);

        if (data == null)
        {
            Debug.LogWarning("[ADVENTURER SPAWNER] Cannot restore adventurer. Missing data id: " + adventurerId);
            return null;
        }

        Transform point = queueIndex >= 0 ? GetQueuePoint(Mathf.Clamp(queueIndex, 0, QueueCapacity - 1)) : spawnPoint;

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

    // This will be useful before loading save data.
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
}