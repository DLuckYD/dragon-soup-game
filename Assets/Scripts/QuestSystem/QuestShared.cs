using System;
using UnityEngine;

public enum AdventurerState
{   Offered, 
    InProgress,
    WaitingReward
}

public enum QuestState
{
    Offered,
    InProgress,
    WaitingReward,
    Completed,
    Failed
}

[Serializable]
public class QuestRequest
{
    public IngredientData ingredient;
    public int amount;
    public int minRewardValue;
    public float returnDelaySeconds;
}

[Serializable]
public class QuestInstance
{
    public AdventurerNPC npc;
    public AdventurerData adventurerData;
    public QuestRequest questRequests;
    
    public QuestState state;
    public float returnAtTime;
    public int attemptsLeft;
}
