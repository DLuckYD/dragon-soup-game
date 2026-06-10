using System;
using UnityEngine;

/// <summary>
/// Describes the final evaluation result for a reward item.
/// </summary>
public enum RewardEvaluationResult
{
    AutoAccept,
    HaggleRequired,
    Refused
}

/// <summary>
/// Stores all information about a reward evaluation.
/// This is useful for UI feedback, debugging, and future balancing.
/// </summary>
public struct RewardEvaluation
{
    public RewardEvaluationResult result;

    public int baseValue;
    public int score;
    public int difficulty;

    public bool itemMatches;
    public bool stateMatches;

    public string itemId;
    public ItemState itemState;

    public string message;
}

/// <summary>
/// Handles reward evaluation and haggle dice logic.
/// 
/// Formula:
/// rewardScore = RewardItem.Value + item preference bonus + state preference bonus
/// 
/// Result:
/// score >= difficulty         -> automatic success
/// score == difficulty - 1     -> haggle required
/// score < difficulty - 1      -> refused
/// 
/// Haggle:
/// Roll D20.
/// roll > 10  -> success
/// roll <= 10 -> fail
/// </summary>
public static class RewardEvaluator
{
    private const int ItemPreferenceBonus = 1;
    private const int StatePreferenceBonus = 1;

    private const int MinDifficulty = 1;
    private const int DiceMinInclusive = 1;
    private const int DiceMaxExclusive = 21;
    private const int HaggleSuccessThreshold = 10;

    /// <summary>
    /// Evaluates whether the selected inventory item is accepted by the adventurer.
    /// </summary>
    /// <param name="inventoryItem">The item currently selected or held by the player.</param>
    /// <param name="adventurer">The adventurer who evaluates the reward.</param>
    /// <param name="difficultyLevel">The quest or recipe difficulty level.</param>
    /// <returns>Full reward evaluation data.</returns>
    public static RewardEvaluation EvaluateReward(
        InventoryItem inventoryItem,
        AdventurerData adventurer,
        int difficultyLevel)
    {
        RewardEvaluation evaluation = new RewardEvaluation
        {
            result = RewardEvaluationResult.Refused,
            baseValue = 0,
            score = 0,
            difficulty = Mathf.Max(MinDifficulty, difficultyLevel),
            itemMatches = false,
            stateMatches = false,
            itemId = string.Empty,
            itemState = ItemState.None,
            message = string.Empty
        };

        if (inventoryItem == null)
        {
            evaluation.message = "No item was selected.";
            return evaluation;
        }

        RewardItem rewardItem = inventoryItem as RewardItem;

        if (rewardItem == null)
        {
            evaluation.message = "This item cannot be used as a reward.";
            return evaluation;
        }

        if (rewardItem.itemData == null)
        {
            evaluation.message = "Reward item has no item data.";
            return evaluation;
        }

        if (adventurer == null)
        {
            evaluation.message = "Adventurer data is missing.";
            return evaluation;
        }

        string rewardItemId = rewardItem.itemData.id;
        ItemState rewardItemState = rewardItem.CurrentState;

        evaluation.itemId = rewardItemId;
        evaluation.itemState = rewardItemState;
        evaluation.baseValue = rewardItem.Value;

        int score = rewardItem.Value;

        if (string.Equals(
                rewardItemId,
                adventurer.preferredRewardItemId,
                StringComparison.OrdinalIgnoreCase))
        {
            score += ItemPreferenceBonus;
            evaluation.itemMatches = true;
        }

        if (rewardItemState == adventurer.preferredItemState)
        {
            score += StatePreferenceBonus;
            evaluation.stateMatches = true;
        }

        evaluation.score = score;

        if (score >= evaluation.difficulty)
        {
            evaluation.result = RewardEvaluationResult.AutoAccept;
            evaluation.message = "The adventurer accepts this reward.";
            return evaluation;
        }

        if (score == evaluation.difficulty - 1)
        {
            evaluation.result = RewardEvaluationResult.HaggleRequired;
            evaluation.message = "The adventurer is unsure. Haggle is required.";
            return evaluation;
        }

        evaluation.result = RewardEvaluationResult.Refused;
        evaluation.message = "The adventurer refuses this reward.";
        return evaluation;
    }

    /// <summary>
    /// Rolls a D20 dice.
    /// Returns a number from 1 to 20.
    /// </summary>
    public static int RollD20()
    {
        return UnityEngine.Random.Range(DiceMinInclusive, DiceMaxExclusive);
    }

    /// <summary>
    /// Checks whether a haggle dice roll is successful.
    /// Current rule:
    /// roll > 10 means success.
    /// </summary>
    public static bool IsHaggleSuccess(int roll)
    {
        return roll > HaggleSuccessThreshold;
    }

    /// <summary>
    /// Builds a short debug string for the reward evaluation.
    /// Useful for Debug.Log and temporary UI.
    /// </summary>
    public static string BuildDebugMessage(RewardEvaluation evaluation)
    {
        return
            $"Reward Evaluation:\n" +
            $"Result: {evaluation.result}\n" +
            $"Item ID: {evaluation.itemId}\n" +
            $"Item State: {evaluation.itemState}\n" +
            $"Base Value: {evaluation.baseValue}\n" +
            $"Item Matches: {evaluation.itemMatches}\n" +
            $"State Matches: {evaluation.stateMatches}\n" +
            $"Final Score: {evaluation.score}\n" +
            $"Difficulty: {evaluation.difficulty}\n" +
            $"Message: {evaluation.message}";
    }
}