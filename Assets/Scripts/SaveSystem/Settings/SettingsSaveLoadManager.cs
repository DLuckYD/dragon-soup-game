using System;
using System.IO;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class SettingsSaveLoadManager : MonoBehaviour
{
    public static SettingsSaveLoadManager Instance { get; private set; }

    public SettingsData CurrentSettings { get; private set; }

    private string settingsFolderPath;
    private string settingsFilePath;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // can exist between scenes, so we can load/save settings from anywhere
        DontDestroyOnLoad(gameObject);

        // path of the save file
        settingsFolderPath = Path.Combine(Application.persistentDataPath, "Settings");
        settingsFilePath = Path.Combine(settingsFolderPath, "settings.json");

#if UNITY_EDITOR
        Debug.Log(settingsFilePath);
#endif

        LoadSettings();
        ApplySettings();
    }

    // load the settings from the file
    public void LoadSettings()
    {
        // if there is no file, create a new one with default values
        if (!File.Exists(settingsFilePath))
        {
            CreateDefaultSettings();
            return;
        }

        // check if the file can be found and opened
        try
        {
            string json = File.ReadAllText(settingsFilePath);
            CurrentSettings = JsonUtility.FromJson<SettingsData>(json);

            if (CurrentSettings == null)
            {
                Debug.LogWarning("Settings file was empty or invalid. Creating default settings.");
                CreateDefaultSettings();
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Failed to load settings. Creating default settings. Error: " + e.Message);
            CreateDefaultSettings();
        }
    }

    public void SaveSettings()
    {
        try
        {
            if (!Directory.Exists(settingsFolderPath))
                Directory.CreateDirectory(settingsFolderPath);

            string json = JsonUtility.ToJson(CurrentSettings, true);
            File.WriteAllText(settingsFilePath, json);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to save settings: " + e.Message);
        }
    }

    private void CreateDefaultSettings()
    {
        CurrentSettings = new SettingsData();
        SaveSettings();
    }

    // apply the settings to the game
    public void ApplySettings()
    {

        if (CurrentSettings == null)
            return;


        // audio volume
        // resolution
        // fullscreen
        // language
        // mouse sensitivity
    }
}
