using System.Collections.Generic;

[System.Serializable]
public class QuestSaveData
{
    public List<AdventurerQuestSaveData> adventurers = new List<AdventurerQuestSaveData>();

    // Current position inside the quest target bag.
    // This keeps quest generation progress after load.
    public int bagIndex;
}

[System.Serializable]
public class AdventurerQuestSaveData
{
    public string adventurerId;

    // Stored as string for readable JSON:
    // "Offered", "InProgress", "WaitingReward"
    public string state;

    // Queue position near the window.
    // 0 = first position, interactable
    // 1 = second position, waiting
    // -1 = not currently in the visible queue
    public int queueIndex = -1;

    public bool hasActiveQuest;
    public ActiveQuestSaveData activeQuest;
}

[System.Serializable]
public class ActiveQuestSaveData
{
    public string ingredientId;
    public int amount;
    public int minRewardValue;

    // We do not save returnAtTime directly because it depends on Time.time.
    // Instead, we save how much time is still left.
    public float remainingReturnSeconds;

    public int attemptsLeft;
}