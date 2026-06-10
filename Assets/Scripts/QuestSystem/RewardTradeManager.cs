using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Result of submitting a reward item to an adventurer.
/// This does not finish the quest by itself.
/// QuestManager decides what to do with this result.
/// </summary>
public enum RewardTradeSubmitOutcome
{
    Accepted,
    HaggleRequired,
    Refused
}

/// <summary>
/// Result of rolling the D20 during haggle.
/// </summary>
public enum HaggleRollOutcome
{
    NoPendingHaggle,
    Success,
    FailedCanRetry,
    FailedNoAttemptsLeft
}

/// <summary>
/// Data returned after the player submits a reward item.
/// Used by QuestManager and UI.
/// </summary>
public struct RewardTradeSubmitResult
{
    public RewardTradeSubmitOutcome outcome;

    public RewardEvaluation evaluation;

    public RewardItem rewardItem;

    public int haggleAttemptsLeft;

    public string message;
}

/// <summary>
/// Data returned after the player rolls the haggle dice.
/// Used by QuestManager and UI.
/// </summary>
public struct HaggleRollResult
{
    public HaggleRollOutcome outcome;

    public int roll;
    public bool success;

    public RewardItem rewardItem;

    public int attemptsLeft;

    public string message;
}

/// <summary>
/// Runtime haggle/trade session for one adventurer.
/// This is temporary runtime state and should not be saved directly.
/// </summary>
public class RewardTradeSession
{
    public AdventurerNPC npc;

    public RewardItem pendingRewardItem;

    public RewardEvaluation lastEvaluation;

    public int difficultyLevel;
    public int attemptsLeft;
}

/// <summary>
/// Handles the reward agreement flow:
/// - reward item submission
/// - reward evaluation
/// - haggle session state
/// - D20 rolling
/// - haggle attempt tracking
/// 
/// This manager does NOT:
/// - spawn ingredients
/// - finish quests
/// - remove adventurers
/// - manage quest timers
/// 
/// Those responsibilities remain in QuestManager.
/// </summary>
public class RewardTradeManager : MonoBehaviour
{
    public const int DefaultHaggleAttempts = 2;

    private readonly Dictionary<AdventurerNPC, RewardTradeSession> activeSessions = new();

    /// <summary>
    /// Submits a reward item for evaluation.
    /// 
    /// Possible results:
    /// - Accepted: QuestManager should consume the item, give ingredient, and finish the quest.
    /// - HaggleRequired: UI should show haggle option and Roll D20 button.
    /// - Refused: UI should show refusal feedback. No haggle attempt is consumed.
    /// </summary>
    public RewardTradeSubmitResult SubmitReward(
        AdventurerNPC npc,
        InventoryItem inventoryItem,
        int difficultyLevel)
    {
        RewardTradeSubmitResult result = new RewardTradeSubmitResult
        {
            outcome = RewardTradeSubmitOutcome.Refused,
            evaluation = default,
            rewardItem = null,
            haggleAttemptsLeft = GetAttemptsLeft(npc),
            message = string.Empty
        };

        if (npc == null)
        {
            result.message = "NPC is missing.";
            return result;
        }

        if (inventoryItem == null)
        {
            ClearPendingItem(npc);

            result.message = "No reward item was selected.";
            result.haggleAttemptsLeft = GetAttemptsLeft(npc);
            return result;
        }

        int safeDifficulty = Mathf.Max(1, difficultyLevel);

        RewardEvaluation evaluation = RewardEvaluator.EvaluateReward(
            inventoryItem,
            npc.Data,
            safeDifficulty
        );

        result.evaluation = evaluation;
        result.message = evaluation.message;

        RewardItem rewardItem = inventoryItem as RewardItem;

        if (evaluation.result == RewardEvaluationResult.AutoAccept)
        {
            // The reward is good enough.
            // Clear any old haggle session because the trade will be completed now.
            ClearSession(npc);

            result.outcome = RewardTradeSubmitOutcome.Accepted;
            result.rewardItem = rewardItem;
            result.haggleAttemptsLeft = 0;
            return result;
        }

        RewardTradeSession session = GetOrCreateSession(npc, safeDifficulty);

        session.lastEvaluation = evaluation;
        session.difficultyLevel = safeDifficulty;

        if (evaluation.result == RewardEvaluationResult.HaggleRequired)
        {
            // The reward is close enough, but the adventurer needs to be convinced.
            // Store this exact item. If the D20 roll succeeds, this item will be consumed.
            session.pendingRewardItem = rewardItem;

            result.outcome = RewardTradeSubmitOutcome.HaggleRequired;
            result.rewardItem = rewardItem;
            result.haggleAttemptsLeft = session.attemptsLeft;
            return result;
        }

        // Refused:
        // The item is too bad or invalid.
        // Important: this does NOT consume a haggle attempt.
        // We clear only the pending item, but keep the session attempts.
        session.pendingRewardItem = null;

        result.outcome = RewardTradeSubmitOutcome.Refused;
        result.rewardItem = rewardItem;
        result.haggleAttemptsLeft = session.attemptsLeft;
        return result;
    }

    /// <summary>
    /// Rolls D20 for the current pending haggle item.
    /// 
    /// If successful:
    /// - returns Success
    /// - QuestManager should consume the pending reward item and finish the quest successfully.
    /// 
    /// If failed:
    /// - consumes one haggle attempt
    /// - if attempts remain, returns FailedCanRetry
    /// - if no attempts remain, returns FailedNoAttemptsLeft
    /// </summary>
    public HaggleRollResult RollHaggle(AdventurerNPC npc)
    {
        HaggleRollResult result = new HaggleRollResult
        {
            outcome = HaggleRollOutcome.NoPendingHaggle,
            roll = 0,
            success = false,
            rewardItem = null,
            attemptsLeft = GetAttemptsLeft(npc),
            message = string.Empty
        };

        if (npc == null)
        {
            result.message = "NPC is missing.";
            return result;
        }

        if (!activeSessions.TryGetValue(npc, out RewardTradeSession session) || session == null)
        {
            result.message = "No active reward trade session.";
            return result;
        }

        if (session.pendingRewardItem == null)
        {
            result.message = "No pending haggle item.";
            result.attemptsLeft = session.attemptsLeft;
            return result;
        }

        if (session.attemptsLeft <= 0)
        {
            result.outcome = HaggleRollOutcome.FailedNoAttemptsLeft;
            result.message = "No haggle attempts left.";
            result.rewardItem = session.pendingRewardItem;
            result.attemptsLeft = 0;

            ClearSession(npc);
            return result;
        }

        int roll = RewardEvaluator.RollD20();
        bool success = RewardEvaluator.IsHaggleSuccess(roll);

        result.roll = roll;
        result.success = success;
        result.rewardItem = session.pendingRewardItem;

        if (success)
        {
            // Successful haggle means the adventurer accepts the pending item.
            result.outcome = HaggleRollOutcome.Success;
            result.message = "Haggle succeeded.";
            result.attemptsLeft = session.attemptsLeft;

            ClearSession(npc);
            return result;
        }

        // Failed roll consumes one attempt.
        session.attemptsLeft--;

        result.attemptsLeft = session.attemptsLeft;

        if (session.attemptsLeft <= 0)
        {
            // Failed twice. Adventurer should leave without giving the ingredient.
            result.outcome = HaggleRollOutcome.FailedNoAttemptsLeft;
            result.message = "Haggle failed. No attempts left.";

            ClearSession(npc);
            return result;
        }

        // First fail. Player can reroll once.
        result.outcome = HaggleRollOutcome.FailedCanRetry;
        result.message = "Haggle failed. Player can reroll.";

        return result;
    }

    /// <summary>
    /// Returns remaining haggle attempts for this adventurer.
    /// If no session exists yet, returns the default amount.
    /// </summary>
    public int GetAttemptsLeft(AdventurerNPC npc)
    {
        if (npc != null && activeSessions.TryGetValue(npc, out RewardTradeSession session) && session != null)
        {
            return session.attemptsLeft;
        }

        return DefaultHaggleAttempts;
    }

    /// <summary>
    /// Returns true if this adventurer currently has a pending item waiting for a D20 roll.
    /// </summary>
    public bool HasPendingHaggle(AdventurerNPC npc)
    {
        return npc != null
               && activeSessions.TryGetValue(npc, out RewardTradeSession session)
               && session != null
               && session.pendingRewardItem != null;
    }

    /// <summary>
    /// Removes the pending item but keeps the session and remaining attempts.
    /// Useful when the player shows a refused item.
    /// </summary>
    public void ClearPendingItem(AdventurerNPC npc)
    {
        if (npc == null)
            return;

        if (activeSessions.TryGetValue(npc, out RewardTradeSession session) && session != null)
        {
            session.pendingRewardItem = null;
        }
    }

    /// <summary>
    /// Clears the full reward trade session for one adventurer.
    /// Call this when the quest is finished, dismissed, or reset.
    /// </summary>
    public void ClearSession(AdventurerNPC npc)
    {
        if (npc == null)
            return;

        activeSessions.Remove(npc);
    }

    /// <summary>
    /// Clears all active trade sessions.
    /// Call this when loading a save or resetting quest runtime state.
    /// </summary>
    public void ClearAllSessions()
    {
        activeSessions.Clear();
    }

    private RewardTradeSession GetOrCreateSession(AdventurerNPC npc, int difficultyLevel)
    {
        if (activeSessions.TryGetValue(npc, out RewardTradeSession existingSession) && existingSession != null)
        {
            return existingSession;
        }

        RewardTradeSession session = new RewardTradeSession
        {
            npc = npc,
            pendingRewardItem = null,
            lastEvaluation = default,
            difficultyLevel = Mathf.Max(1, difficultyLevel),
            attemptsLeft = DefaultHaggleAttempts
        };

        activeSessions[npc] = session;

        return session;
    }
    
    public void RestoreSessionState(AdventurerNPC npc, int difficultyLevel, int attemptsLeft)
    {
        if (npc == null)
            return;

        RewardTradeSession session = new RewardTradeSession
        {
            npc = npc,
            pendingRewardItem = null,

            // We restore only attempts and difficulty.
            // The pending item is temporary runtime/UI state and is not restored.
            lastEvaluation = default,
            difficultyLevel = Mathf.Max(1, difficultyLevel),
            attemptsLeft = Mathf.Clamp(attemptsLeft, 0, DefaultHaggleAttempts)
        };

        activeSessions[npc] = session;
    }
}