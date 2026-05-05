using UnityEngine;

using System.Collections.Generic;

[System.Serializable]
public class QuestSaveData
{
    public List<AdventurerQuestSaveData> adventurers = new List<AdventurerQuestSaveData>();
    public int bagIndex;
}

[System.Serializable]
public class AdventurerQuestSaveData
{
    public string adventurerId;
    public string state;

    public bool hasActiveQuest;
    public ActiveQuestSaveData activeQuest;
}

[System.Serializable]
public class ActiveQuestSaveData
{
    public string ingredientId;
    public int amount;
    public int minRewardValue;

    public float remainingReturnSeconds;
    public int attemptsLeft;
}
