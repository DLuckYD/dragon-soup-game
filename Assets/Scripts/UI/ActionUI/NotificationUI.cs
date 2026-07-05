using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NotificationUI : MonoBehaviour
{
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TMP_Text notificationText;

    [Header("Timing")]
    [SerializeField] private float showTime = 2f;
    [SerializeField] private float minimumVisibleTime = 0.3f;

    [Header("Queue")]
    [SerializeField] private int maxQueueSize = 10;

    private readonly Queue<string> messageQueue = new Queue<string>();
    private Coroutine currentRoutine;

    private void Awake()
    {
        Debug.Log("[NotificationUI] Awake");
    }

    private void OnEnable()
    {
        Debug.Log("Subscribing to save events...");

        GameSaveLoadManager.OnAutoSaveCompleted += ShowMessage;
        GameSaveLoadManager.OnManualSaveCompleted += ShowMessage;
        GameSaveLoadManager.OnSaveFailed += ShowMessage;

        UpgradeStation.OnSuccessfulUpgrade += ShowMessage;
        UpgradeStation.OnUnsuccessfulUpgrade += ShowMessage;

        RecipeProgressManager.OnSuccessfulUnlock += ShowMessage;

        HouseSpawner.OnSpawned += ShowMessage;

        PlayerInteraction.OnLockedItemInteraction += ShowMessage;
        PlayerInteraction.OnFullInventory += ShowMessage;

        AdventurerSpawner.OnSpawned += ShowMessage;
        AdventurerSpawner.OnReturn += ShowMessage;

        DarkEntity.OnSpawned += ShowMessage;

        CookingStation.OnMissingIngredients += ShowMessage;
        CookingStation.OnNextIngredient += ShowMessage;
        CookingStation.OnInventoryFull += ShowMessage;
    }

    private void OnDisable()
    {
        Debug.Log("Unsubscribing from save events...");

        GameSaveLoadManager.OnAutoSaveCompleted -= ShowMessage;
        GameSaveLoadManager.OnManualSaveCompleted -= ShowMessage;
        GameSaveLoadManager.OnSaveFailed -= ShowMessage;

        UpgradeStation.OnSuccessfulUpgrade -= ShowMessage;
        UpgradeStation.OnUnsuccessfulUpgrade -= ShowMessage;

        RecipeProgressManager.OnSuccessfulUnlock -= ShowMessage;

        HouseSpawner.OnSpawned -= ShowMessage;

        PlayerInteraction.OnLockedItemInteraction -= ShowMessage;
        PlayerInteraction.OnFullInventory -= ShowMessage;

        AdventurerSpawner.OnSpawned -= ShowMessage;
        AdventurerSpawner.OnReturn -= ShowMessage;

        DarkEntity.OnSpawned -= ShowMessage;

        CookingStation.OnMissingIngredients -= ShowMessage;
        CookingStation.OnNextIngredient -= ShowMessage;
        CookingStation.OnInventoryFull -= ShowMessage;
    }

    private void Start()
    {
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
        }
    }

    private void ShowMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        Debug.Log("Queued notification: " + message);

        if (notificationPanel == null || notificationText == null)
        {
            Debug.LogWarning("[NOTIFICATION UI] Panel or text is not assigned.");
            return;
        }

        EnqueueMessage(message);

        if (currentRoutine == null)
        {
            currentRoutine = StartCoroutine(ProcessMessageQueue());
        }
    }

    private void EnqueueMessage(string message)
    {
        while (messageQueue.Count >= maxQueueSize)
        {
            string removedMessage = messageQueue.Dequeue();
            Debug.Log("[NOTIFICATION UI] Queue is full. Removed oldest message: " + removedMessage);
        }

        messageQueue.Enqueue(message);
    }

    private IEnumerator ProcessMessageQueue()
    {
        notificationPanel.SetActive(true);

        while (messageQueue.Count > 0)
        {
            string message = messageQueue.Dequeue();

            notificationText.text = message;
            Debug.Log("Showing notification: " + message);

            float visibleTime = 0f;

            while (visibleTime < showTime)
            {
                float step = 0.05f;

                yield return new WaitForSecondsRealtime(step);

                visibleTime += step;

                bool canSwitchToNextMessage = visibleTime >= minimumVisibleTime;
                bool hasWaitingMessages = messageQueue.Count > 0;

                if (canSwitchToNextMessage && hasWaitingMessages)
                {
                    break;
                }
            }
        }

        notificationPanel.SetActive(false);
        notificationText.text = "";

        currentRoutine = null;
    }
}