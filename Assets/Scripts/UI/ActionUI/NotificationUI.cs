using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class NotificationUI : MonoBehaviour
{
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TMP_Text notificationText;
    [SerializeField] private float showTime = 2f;

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

        HouseSpawner.OnSpawned  += ShowMessage;

        PlayerInteraction.OnLockedItemInteraction += ShowMessage;
        PlayerInteraction.OnFullInventory += ShowMessage;

        AdventurerSpawner.OnSpawned += ShowMessage;
        AdventurerSpawner.OnReturn += ShowMessage;

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

        HouseSpawner.OnSpawned  -= ShowMessage;

        PlayerInteraction.OnLockedItemInteraction -= ShowMessage;
        PlayerInteraction.OnFullInventory -= ShowMessage;

        AdventurerSpawner.OnSpawned -= ShowMessage;
        AdventurerSpawner.OnReturn -= ShowMessage;

    }

    void Start()
    {
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
        }
    }

    //private void ShowAutoSaveMessage(string message)
    //{
    //    Debug.Log("Received auto save message: " + message);
    //    ShowMessage(message);
    //}

    //private void ShowManualSaveMessage(string message)
    //{
    //    Debug.Log("Received manual save message: " + message);
    //    ShowMessage(message);
    //}

    //private void ShowSaveFailedMessage(string message)
    //{
    //    Debug.Log("Received save failed message: " + message);
    //    ShowMessage(message);
    //}

    //private void ShowSupplyMessage(string message)
    //{
    //    Debug.Log("Received supply message: " + message);
    //    ShowMessage(message);
    //}
    //private void ShowUpgradeMessage(string message)
    //{
    //    Debug.Log("Received upgrade message: " + message);
    //    ShowMessage(message);
    //}

    //private void ShowUnlockMessage(string message)
    //{
    //    Debug.Log("Received unlock message: " + message);
    //    ShowMessage(message);
    //}

    //private void ShowLockedItemMessage(string message)
    //{
    //    Debug.Log("Received locked message: " + message);
    //    ShowMessage(message);
    //}

    //private void ShowDestroyedMessage(string message)
    //{
    //    Debug.Log("Received destroyed message: " + message);
    //    ShowMessage(message);
    //}
    

    private void ShowMessage(string message)
    {
        Debug.Log("Showing notification: " + message);

        if (notificationPanel == null || notificationText == null)
        {
            Debug.LogWarning("[NOTIFICATION UI] Panel or text is not assigned.");
            return;
        }

        notificationText.text = message;

        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }

        currentRoutine = StartCoroutine(ShowNotificationCoroutine());
    }

    private IEnumerator ShowNotificationCoroutine()
    {
        notificationPanel.SetActive(true);

        yield return new WaitForSecondsRealtime(showTime);

        notificationPanel.SetActive(false);
        notificationText.text = "";
        currentRoutine = null;
    }
}
