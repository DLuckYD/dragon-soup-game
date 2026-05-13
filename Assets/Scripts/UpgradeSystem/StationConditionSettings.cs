using System;
using System.Collections.Generic;
using UnityEngine;

public enum ItemStateRequirement
{
    Any,        // Состояние не важно
    MustBeNone, // Предмет должен быть без состояния
    MustBe,    // Предмет должен иметь конкретное состояние
    MustNotBe  // Предмет НЕ должен иметь конкретное состояние
}

[Serializable]
public class StationConditionSettings
{
    [Header("Basic Item Conditions")]

    [SerializeField] private bool requireCanBeModified = true;

    [Header("State Conditions")]

    [SerializeField] private ItemStateRequirement stateRequirement = ItemStateRequirement.MustBeNone;

    [SerializeField] private ItemState requiredState = ItemState.None;

    [Header("Type Conditions")]

    [SerializeField] private bool useTypeFilter = false;

    [SerializeField] private List<ItemType> allowedTypes = new List<ItemType>();

    public bool IsValid(RewardItem item, out string failMessage)
    {
        failMessage = string.Empty;

        if (item == null)
        {
            failMessage = "No item provided.";
            return false;
        }

        // Проверка: можно ли вообще модифицировать этот предмет.
        if (requireCanBeModified && !item.canBeModified)
        {
            failMessage = "This item cannot be modified.";
            return false;
        }

        // Проверка текущего состояния предмета.
        switch (stateRequirement)
        {
            case ItemStateRequirement.Any:
                break;

            case ItemStateRequirement.MustBeNone:
                if (item.HasAnyState())
                {
                    failMessage = $"Item already has state: {item.CurrentState}.";
                    return false;
                }
                break;

            case ItemStateRequirement.MustBe:
                if (!item.HasState(requiredState))
                {
                    failMessage = $"Item must have state: {requiredState}.";
                    return false;
                }
                break;

            case ItemStateRequirement.MustNotBe:
                if (item.HasState(requiredState))
                {
                    failMessage = $"Item must not have state: {requiredState}.";
                    return false;
                }
                break;
        }

        // Проверка типа предмета.
        if (useTypeFilter)
        {
            if (allowedTypes == null || allowedTypes.Count == 0)
            {
                failMessage = "No allowed item types configured.";
                return false;
            }

            if (!allowedTypes.Contains(item.type))
            {
                failMessage = $"Item type {item.type} is not allowed for this station.";
                return false;
            }
        }

        return true;
    }
}