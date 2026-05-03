using System.Collections;
using TMPro;
using UnityEngine;

public class SaveNotificationUI : MonoBehaviour
{
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TMP_Text notificationText;
    [SerializeField] private float showTime = 2f;

    private Coroutine currentRoutine;

    private void OnEnable()
    {
        Debug.Log("Subscribing to save events...");
        GameSaveLoadManager.OnAutoSaveCompleted += ShowAutoSaveMessage;
        GameSaveLoadManager.OnManualSaveCompleted += ShowManualSaveMessage;
    }

    private void OnDisable()
    {
        Debug.Log("Unsubscribing from save events...");
        GameSaveLoadManager.OnAutoSaveCompleted -= ShowAutoSaveMessage;
        GameSaveLoadManager.OnManualSaveCompleted -= ShowManualSaveMessage;
    }

    void Start()
    {
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
        }
    }

    private void ShowAutoSaveMessage(string message)
    {
        Debug.Log("Received auto save message: " + message);
        ShowMessage(message);
    }

    private void ShowManualSaveMessage(string message)
    {
        Debug.Log("Received manual save message: " + message);
        ShowMessage(message);
    }

    private void ShowMessage(string message)
    {
        Debug.Log("Showing save notification: " + message);
        if (notificationPanel == null || notificationText == null)
            return;

        notificationText.text = message;
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }
        Debug.Log("Starting notification coroutine...");
        currentRoutine = StartCoroutine(ShowNotificationCoroutine());
    }

    private IEnumerator ShowNotificationCoroutine()
    {
        notificationPanel.SetActive(true);
        yield return new WaitForSeconds(showTime);
        notificationPanel.SetActive(false);
        notificationText.text = "";
        currentRoutine = null;
    }
}
