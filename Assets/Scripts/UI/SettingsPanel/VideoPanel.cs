using UnityEngine;

using System.Collections.Generic;
using TMPro;

public class VideoSettings : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_Dropdown screenModeDropdown;

    private Resolution[] resolutions;
    private List<Resolution> uniqueResolutions = new List<Resolution>();
    private int currentResolutionIndex = 0;

    void Start()
    {
        SetAllPossibleResolutions();
        SetAllPossibleScreenModes();

        resolutionDropdown.onValueChanged.AddListener(SetResolution);
        screenModeDropdown.onValueChanged.AddListener(SetScreenMode);
    }

    public void SetResolution(int resolutionIndex)
    {
        if (resolutionIndex < 0 || resolutionIndex >= uniqueResolutions.Count)
            return;
        Resolution resolution = uniqueResolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, true);
    }

    public void SetAllPossibleResolutions()
    {
        resolutions = Screen.resolutions;
        uniqueResolutions.Clear();
        resolutionDropdown.ClearOptions();

        HashSet<string> addedResolutions = new HashSet<string>();
        List<string> options = new List<string>();

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

        currentResolutionIndex = 0;

        for (int i = 0; i < uniqueResolutions.Count; i++)
        {
            options.Add(uniqueResolutions[i].width + "x" + uniqueResolutions[i].height);

            if (uniqueResolutions[i].width == Screen.width &&
                uniqueResolutions[i].height == Screen.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    public void SetScreenMode(int screenModeIndex)
    {
        FullScreenMode mode = FullScreenMode.FullScreenWindow;

        switch (screenModeIndex)
        {
            case 0:
                mode = FullScreenMode.ExclusiveFullScreen;
                break;
            case 1:
                mode = FullScreenMode.FullScreenWindow;
                break;
            case 2:
                mode = FullScreenMode.Windowed;
                break;
            case 3:
                mode = FullScreenMode.MaximizedWindow;
                break;
        }

        Screen.SetResolution(Screen.width, Screen.height, mode);
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
        screenModeDropdown.value = GetCurrentScreenModeIndex();
        screenModeDropdown.RefreshShownValue();
    }

    private int GetCurrentScreenModeIndex()
    {
        switch (Screen.fullScreenMode)
        {
            case FullScreenMode.ExclusiveFullScreen:
                return 0;
            case FullScreenMode.FullScreenWindow:
                return 1;
            case FullScreenMode.Windowed:
                return 2;
            case FullScreenMode.MaximizedWindow:
                return 3;
            default:
                return 1;
        }
    }
}