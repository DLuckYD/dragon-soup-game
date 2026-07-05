using TMPro;
using UnityEngine;

public class NavigationNotificationUI : MonoBehaviour
{
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TMP_Text notificationText;

    private void Start()
    {
        Debug.Log("[NavigationNotificationUI] Start");
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("[NavigationNotificationUI] Notification panel is not assigned.");
        }
    }

    private void OnEnable()
    {
        Debug.Log("Subscribing to [NavigationNotificationUI] events...");
        PlayerInteraction.OnInteraction += ShowMessage;
        PlayerInteraction.OnEndedInteraction += HideMessage;

        //DismantlingManager.OnInteraction += ShowMessage;
        //DismantlingManager.OnEndedInteraction += HideMessage;
    }

    private void OnDisable()
    {
        Debug.Log("Unsubscribing from [NavigationNotificationUI] events...");
        PlayerInteraction.OnInteraction -= ShowMessage;
        PlayerInteraction.OnEndedInteraction -= HideMessage;

        //DismantlingManager.OnInteraction -= ShowMessage;
        //DismantlingManager.OnEndedInteraction -= HideMessage;
    }

    private void ShowMessage(string message)
    {
        Debug.Log("Showing interaction prompt: " + message);

        if (notificationPanel == null || notificationText == null)
        {
            Debug.LogWarning("[NOTIFICATION UI] Panel or text is not assigned.");
            return;
        }

        notificationText.text = message;
        notificationPanel.SetActive(true);
    }

    private void HideMessage()
    {
        if (notificationPanel == null || notificationText == null)
        {
            Debug.LogWarning("[NOTIFICATION UI] Panel or text is not assigned.");
            return;
        }

        notificationText.text = "";
        notificationPanel.SetActive(false);
    }
}
