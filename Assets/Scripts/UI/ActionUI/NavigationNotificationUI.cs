using TMPro;
using UnityEngine;

public class NavigationNotificationUI : MonoBehaviour
{
    [SerializeField] private GameObject notificationPanel;

    [SerializeField] private TMP_Text prefixText;
    [SerializeField] private TMP_Text keyText;
    [SerializeField] private TMP_Text actionText;

    private void Awake()
    {
        if (notificationPanel != null)
            notificationPanel.SetActive(false);
    }

    public void ShowMessage(string prefix, string key, string action)
    {
        if (notificationPanel == null ||
            prefixText == null ||
            keyText == null ||
            actionText == null)
        {
            Debug.LogWarning("[TUTORIAL UI] UI references are missing.");
            return;
        }

        prefixText.text = prefix;
        keyText.text = key;
        actionText.text = action;

        notificationPanel.SetActive(true);
    }

    public void HideMessage()
    {
        if (notificationPanel == null)
            return;

        notificationPanel.SetActive(false);
    }
}