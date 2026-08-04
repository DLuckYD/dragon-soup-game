using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VideoSettings : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_Dropdown screenModeDropdown;

    private List<Resolution> uniqueResolutions = new List<Resolution>();

    void Start()
    {
        SetAllPossibleResolutions();
        SetAllPossibleScreenModes();

        LoadValuesFromSettings();

        resolutionDropdown.onValueChanged.AddListener(SetResolution);
        screenModeDropdown.onValueChanged.AddListener(SetScreenMode);
    }

    public void SetResolution(int resolutionIndex)
    {
        if (resolutionIndex < 0 || resolutionIndex >= uniqueResolutions.Count)
            return;

        Resolution resolution = uniqueResolutions[resolutionIndex];

        // get current screen mode
        int screenModeIndex = SettingsSaveLoadManager.Instance.CurrentSettings.screenModeIndex;
        FullScreenMode mode = GetScreenModeFromIndex(screenModeIndex);

        // apply correct resolution with the correct screen mode
        Screen.SetResolution(resolution.width, resolution.height, mode);

        // add new values to file
        SettingsSaveLoadManager.Instance.CurrentSettings.resolutionWidth = resolution.width;
        SettingsSaveLoadManager.Instance.CurrentSettings.resolutionHeight = resolution.height;
        SettingsSaveLoadManager.Instance.SaveSettings();
    }

    public void SetAllPossibleResolutions()
    {
        Resolution[] resolutions = Screen.resolutions;

        uniqueResolutions.Clear();
        resolutionDropdown.ClearOptions();

        HashSet<string> addedResolutions = new HashSet<string>();

        for (int i = 0; i < resolutions.Length; i++)
        {
            string key = resolutions[i].width + "x" + resolutions[i].height;

            if (!addedResolutions.Contains(key))
            {
                addedResolutions.Add(key);
                uniqueResolutions.Add(resolutions[i]);
            }
        }

        uniqueResolutions.Sort((a, b) =>
        {
            if (a.width != b.width)
                return b.width.CompareTo(a.width);

            return b.height.CompareTo(a.height);
        });

        // create text options for the dropdown
        List<string> options = new List<string>();

        foreach (Resolution resolution in uniqueResolutions)
        {
            options.Add(resolution.width + "x" + resolution.height);
        }

        resolutionDropdown.AddOptions(options);
    }

    public void SetScreenMode(int screenModeIndex)
    {
        // convert the index to the correct FullScreenMode
        FullScreenMode mode = GetScreenModeFromIndex(screenModeIndex);

        int width = SettingsSaveLoadManager.Instance.CurrentSettings.resolutionWidth;
        int height = SettingsSaveLoadManager.Instance.CurrentSettings.resolutionHeight;

        if (width <= 0 || height <= 0)
        {
            width = Screen.width;
            height = Screen.height;
        }

        Screen.SetResolution(width, height, mode);

        // save new screen mode index to file
        SettingsSaveLoadManager.Instance.CurrentSettings.screenModeIndex = screenModeIndex;
        SettingsSaveLoadManager.Instance.SaveSettings();
    }

    public void SetAllPossibleScreenModes()
    {
        screenModeDropdown.ClearOptions();

        List<string> options = new List<string>
        {
            "Fullscreen",
            "Borderless",
            "Windowed",
            "Maximized"
        };

        screenModeDropdown.AddOptions(options);
    }

    private void LoadValuesFromSettings()
    {
        // get the save data from file
        SettingsData settings = SettingsSaveLoadManager.Instance.CurrentSettings;

        // find the index that corresponds to the saved resolution
        int savedResolutionIndex = FindResolutionIndex(
            settings.resolutionWidth,
            settings.resolutionHeight
        );

        resolutionDropdown.SetValueWithoutNotify(savedResolutionIndex);

        int screenModeIndex = Mathf.Clamp(settings.screenModeIndex, 0, 3);
        screenModeDropdown.SetValueWithoutNotify(screenModeIndex);

        ApplyCurrentVideoSettings();
    }

    private void ApplyCurrentVideoSettings()
    {
        SettingsData settings = SettingsSaveLoadManager.Instance.CurrentSettings;

        int width = settings.resolutionWidth;
        int height = settings.resolutionHeight;

        if (width <= 0 || height <= 0)
        {
            width = Screen.width;
            height = Screen.height;

            settings.resolutionWidth = width;
            settings.resolutionHeight = height;
        }

        FullScreenMode mode = GetScreenModeFromIndex(settings.screenModeIndex);

        Screen.SetResolution(width, height, mode);
    }

    private int FindResolutionIndex(int width, int height)
    {
        for (int i = 0; i < uniqueResolutions.Count; i++)
        {
            if (uniqueResolutions[i].width == width &&
                uniqueResolutions[i].height == height)
            {
                return i;
            }
        }

        return FindResolutionIndex(Screen.width, Screen.height);
    }

    private FullScreenMode GetScreenModeFromIndex(int screenModeIndex)
    {
        switch (screenModeIndex)
        {
            case 0:
                return FullScreenMode.ExclusiveFullScreen;

            case 1:
                return FullScreenMode.FullScreenWindow;

            case 2:
                return FullScreenMode.Windowed;

            case 3:
                return FullScreenMode.MaximizedWindow;

            default:
                return FullScreenMode.FullScreenWindow;
        }
    }
}