using System.Collections.Generic;
using UnityEngine;

public class AdventurerSpawner : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private QuestManager questManager;
    [SerializeField] private QuestUI questUI;

    [Header("Spawn")]
    [SerializeField] private AdventurerNPC adventurerPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private List<AdventurerData> adventurerPool = new();

    [SerializeField] private float minDelay = 8f;
    [SerializeField] private float maxDelay = 20f;

    [SerializeField] private float offerLifetimeSeconds = 25f;
    private float offerExpiresAt = -1f;

    private AdventurerNPC currentNpc;
    private bool spawnScheduled;

    private void Awake()
    {
        if (questManager == null) questManager = FindFirstObjectByType<QuestManager>();
        if (questUI == null) questUI = FindFirstObjectByType<QuestUI>();
    }

    private void OnEnable()
    {
        if (questManager != null)
            questManager.OnQuestFinished += HandleQuestFinished;
    }

    private void OnDisable()
    {
        if (questManager != null)
            questManager.OnQuestFinished -= HandleQuestFinished;

        CancelInvoke(nameof(SpawnNow));
        spawnScheduled = false;
    }

    private void Start()
    {
        ScheduleNextSpawn();
    }

    private void ScheduleNextSpawn()
    {
        // если NPC еще жив/активен — НЕ спавним нового
        if (currentNpc != null) return;

        // чтобы не наслаивались Invoke
        if (spawnScheduled) return;

        float delay = Random.Range(minDelay, maxDelay);
        spawnScheduled = true;
        Invoke(nameof(SpawnNow), delay);
    }

    private void SpawnNow()
    {
        AkUnitySoundEngine.PostEvent("Adventurer_Arrives", gameObject);
        spawnScheduled = false;

        // если пока ждали — NPC уже появился (например руками) → выходим
        if (currentNpc != null) return;

        if (adventurerPrefab == null || spawnPoint == null || questManager == null || questUI == null)
        {
            Debug.LogWarning("AdventurerSpawner: Missing refs (prefab/spawnPoint/questManager/questUI).", this);
            return;
        }

        if (adventurerPool == null || adventurerPool.Count == 0)
        {
            Debug.LogWarning("AdventurerSpawner: adventurerPool is empty.", this);
            return;
        }

        // выбрать валидные данные
        AdventurerData data = null;
        for (int i = 0; i < 20; i++)
        {
            var pick = adventurerPool[Random.Range(0, adventurerPool.Count)];
            if (pick != null) { data = pick; break; }
        }

        if (data == null)
        {
            Debug.LogWarning("AdventurerSpawner: No valid AdventurerData found in pool.", this);
            return;
        }

        currentNpc = Instantiate(adventurerPrefab, spawnPoint.position, spawnPoint.rotation);
        currentNpc.Initialize(data, questManager, questUI);

        offerExpiresAt = Time.time + offerLifetimeSeconds;
    }

    private void HandleQuestFinished(AdventurerNPC npc)
    {
        // событие может прийти от другого npc — но у нас по логике должен быть только один
        if (npc == null) return;

        if (npc == currentNpc)
            currentNpc = null;

        Destroy(npc.gameObject);

        // ✅ только теперь планируем следующего
        ScheduleNextSpawn();
    }
}
