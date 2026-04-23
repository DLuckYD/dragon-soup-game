using System.IO;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

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
        DontDestroyOnLoad(gameObject);

        settingsFolderPath = Path.Combine(Application.persistentDataPath, "Settings");
        settingsFilePath = Path.Combine(settingsFolderPath, "settings.json");
        Debug.Log(Application.persistentDataPath);
        Debug.Log(settingsFolderPath);
        Debug.Log(settingsFilePath);
        LoadSettings();
    }

    public void LoadSettings()
    {
        if (File.Exists(settingsFilePath))
        {
            string json = File.ReadAllText(settingsFilePath);
            CurrentSettings = JsonUtility.FromJson<SettingsData>(json);
        }
        else
        {
            CurrentSettings = new SettingsData();
            SaveSettings();
        }
    }

    public void SaveSettings()
    {
        if (!Directory.Exists(settingsFolderPath))
            Directory.CreateDirectory(settingsFolderPath);

        string json = JsonUtility.ToJson(CurrentSettings, true);
        File.WriteAllText(settingsFilePath, json);
    }
}
