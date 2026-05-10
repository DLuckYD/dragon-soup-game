using UnityEngine;
using UnityEngine.UI;

public class OthersPanelController : MonoBehaviour
{
    [SerializeField] private Button[] timeButtons;
    [SerializeField] private int[] autoSaveMinutes = { 3, 6, 10, 15 };

    private void Start()
    {
        LoadInputSettings();

        Debug.Log("Buttons count: " + timeButtons.Length);

        for (int i = 0; i < timeButtons.Length; i++)
        {
            int capturedIndex = i;
            Button capturedButton = timeButtons[i];

            if (capturedButton == null)
            {
                Debug.LogWarning("[OTHERS PANEL] Button at index " + capturedIndex + " is NULL.");
                continue;
            }

            capturedButton.onClick.AddListener(() => OnTimeButtonClicked(capturedButton, capturedIndex));
        }
    }

    private void LoadInputSettings()
    {
        if (SettingsSaveLoadManager.Instance == null || SettingsSaveLoadManager.Instance.CurrentSettings == null)
        {
            Debug.LogWarning("SettingsManager or CurrentSettings is missing.");
            return;
        }

        SettingsData settings = SettingsSaveLoadManager.Instance.CurrentSettings;

        int currentAutoSaveTime = settings.autoSaveTime;

        if(autoSaveMinutes != null)
        {
            for (int i = 0; i < autoSaveMinutes.Length; i++)
            {
                if (autoSaveMinutes[i] == currentAutoSaveTime)
                {
                    DisableOtherTimeButtons(i);
                    if (timeButtons != null && i < timeButtons.Length && timeButtons[i] != null)
                    {
                        timeButtons[i].interactable = false;
                    }
                    break;
                }
            }
        }
        else
        {
            Debug.LogWarning("[OTHERS PANEL] autoSaveMinutes array is NULL.");
        }
    }

    private void OnTimeButtonClicked(Button clickedButton, int index)
    {
        if (index < 0 || index >= autoSaveMinutes.Length)
        {
            Debug.LogWarning("[OTHERS PANEL] Invalid autosave time index: " + index);
            return;
        }

        Debug.Log($"Auto save time button {index} clicked");

        DisableOtherTimeButtons(index);
        clickedButton.interactable = false;

        if (GameSaveLoadManager.Instance != null)
        {
            int selectedTime = autoSaveMinutes[index];
            GameSaveLoadManager.Instance.SetAutoSaveTime(selectedTime);

            SettingsSaveLoadManager.Instance.CurrentSettings.autoSaveTime = autoSaveMinutes[index];
            SettingsSaveLoadManager.Instance.SaveSettings();
        }
        else
        {
            Debug.LogWarning("[OTHERS PANEL] GameSaveLoadManager.Instance is NULL.");
        }
    }

    private void DisableOtherTimeButtons(int activeIndex)
    {
        for (int i = 0; i < timeButtons.Length; i++)
        {
            if (i != activeIndex && timeButtons[i] != null)
            {
                timeButtons[i].interactable = true;
            }
        }
    }
}