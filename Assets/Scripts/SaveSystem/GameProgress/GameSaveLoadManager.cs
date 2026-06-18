using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSaveLoadManager : MonoBehaviour
{
    public static GameSaveLoadManager Instance { get; private set; }

    private string savesFolderPath;

    // used to store loaded data temporarily until the scene is fully loaded and ready to apply it
    private GameSaveData pendingLoadData;

    [Header("Autosave")]
    [SerializeField] private string gameplaySceneName = "SampleScene";

    private float autoSaveIntervalSeconds = 360f;

    private Coroutine autoSaveCoroutine;

    public static event Action<string> OnAutoSaveCompleted;
    public static event Action<string> OnManualSaveCompleted;
    public static event Action<string> OnSaveFailed;

    private readonly HashSet<string> saveBlockers = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        savesFolderPath = Path.Combine(Application.persistentDataPath, "Saves");
        Debug.Log("Saves folder path: " + savesFolderPath);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            Debug.Log("Manual save...");
            ManualSave();
        }
    }

    public void AddSaveBlocker(string reason)
    {
        if (string.IsNullOrEmpty(reason))
            reason = "Unknown reason";

        saveBlockers.Add(reason);
    }

    public void RemoveSaveBlocker(string reason)
    {
        if (string.IsNullOrEmpty(reason))
            reason = "Unknown reason";

        if (saveBlockers.Remove(reason))
        {
            Debug.Log("[SAVE BLOCK] Removed blocker: " + reason);
        }
    }

    public void LoadTheSaveFile(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogWarning("Save file does not exist: " + path);
            return;
        }

        GameSaveData data = GetSaveFileData(path);

        if (data == null)
        {
            Debug.LogWarning("Invalid save file: " + path);
            return;
        }

        pendingLoadData = data;

        if (SceneManager.GetActiveScene().name != data.sceneName)
        {
            SceneManager.LoadScene(data.sceneName);
        }
        else
        {
            RestoreGameData(data);
            pendingLoadData = null;
        }
    }

    public GameSaveData GetSaveFileData(string path)
    {
        string json = File.ReadAllText(path);
        GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);

        return data;
    }

    public void StartNewGame()
    {
        pendingLoadData = null;
        SceneManager.LoadScene("SampleScene");
    }

    public void ContinueGame()
    {
        string lastSavePath = GetTheLastSaveFilePath();
        if (lastSavePath == null || string.IsNullOrEmpty(lastSavePath))
        {
            AutoSave();
        }
        lastSavePath = GetTheLastSaveFilePath();
        LoadGame(lastSavePath);
    }

    public string GetTheLastSaveFilePath()
    {
        if (!Directory.Exists(savesFolderPath))
            return null;

        string[] saveFiles = Directory.GetFiles(savesFolderPath, "*.json");
        if (saveFiles.Length == 0)
            return null;

        GameSaveData latestData = GetSortedSaveFiles().FirstOrDefault();

        return latestData?.saveName;
    }

    public List<GameSaveData> GetSortedSaveFiles()
    {
        string[] saveFiles = Directory.GetFiles(savesFolderPath, "*.json");

        List<GameSaveData> saves = new List<GameSaveData>();

        foreach (string filePath in saveFiles)
        {
            GameSaveData data = GetSaveFileData(filePath);

            if (data != null)
            {
                saves.Add(data);
            }
        }

        saves = saves
            .OrderByDescending(save => save.savedAt)
            .ToList();

        return saves;
    }

    public bool SaveGame(string saveFileName)
    {
        if (saveBlockers.Count > 0)
        {
            string message = "Cannot save right now.";
            Debug.LogWarning("[SAVE] " + message);

            OnSaveFailed?.Invoke(message);
            return false;
        }

        GameSaveData data = new GameSaveData();

        data.version = 1;
        data.saveName = saveFileName;
        data.sceneName = SceneManager.GetActiveScene().name;
        data.savedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        if (HotbarManager.Instance != null)
        {
            data.inventory = HotbarManager.Instance.CaptureSaveData();
        }

        if (SceneObjectsManager.Instance != null)
        {
            data.sceneObjects = SceneObjectsManager.Instance.CaptureSaveData();
        }

        if (PlayerSaveManager.Instance != null)
        {
            data.player = PlayerSaveManager.Instance.CaptureSaveData();
        }

        if (RecipeSaveManager.Instance != null)
        {
            data.recipe = RecipeSaveManager.Instance.CaptureSaveData();
        }

        if (QuestSaveManager.Instance != null)
        {
            data.quests = QuestSaveManager.Instance.CaptureSaveData();
        }

        WriteSaveFile(saveFileName, data);

        return true;
    }

    public void AutoSave()
    {
        bool saved = SaveGame("autosave");

        if(!saved)
            return;

        OnAutoSaveCompleted?.Invoke("Autosave completed");
    }

    public void SetAutoSaveTime(float minutes)
    {
        Debug.Log("Setting autosave time to " + minutes + " minutes.");
        autoSaveIntervalSeconds = minutes * 60;
        if (autoSaveCoroutine != null)
        {
            StopAutoSave();
            StartAutoSave();
        }
    }

    private void StartAutoSave()
    {
        if (autoSaveCoroutine != null)
            return;

        autoSaveCoroutine = StartCoroutine(AutoSaveRoutine());
        Debug.Log("[AUTOSAVE] Autosave started. Interval: " + autoSaveIntervalSeconds + " seconds.");
    }

    private void StopAutoSave()
    {
        if (autoSaveCoroutine != null)
        {
            StopCoroutine(autoSaveCoroutine);
            autoSaveCoroutine = null;
            Debug.Log("[AUTOSAVE] Autosave stopped.");
        }
        else
        {
            return;
        }
    }

    private IEnumerator AutoSaveRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(autoSaveIntervalSeconds);

            if (SceneManager.GetActiveScene().name != gameplaySceneName)
            {
                Debug.Log("[AUTOSAVE] Skipped. Current scene is not gameplay scene.");
                continue;
            }

            if (HotbarManager.Instance == null)
            {
                Debug.LogWarning("[AUTOSAVE] Skipped. HotbarManager.Instance is NULL.");
                continue;
            }
            if (SceneObjectsManager.Instance == null)
            {
                Debug.LogWarning("[AUTOSAVE] Skipped. SceneObjectsManager.Instance is NULL.");
                continue;
            }
            if (PlayerSaveManager.Instance == null)
            {
                Debug.LogWarning("[AUTOSAVE] Skipped. PlayerSaveManager.Instance is NULL.");
                continue;
            }
            if (RecipeSaveManager.Instance == null)
            {
                Debug.LogWarning("[AUTOSAVE] Skipped. RecipeSaveManager.Instance is NULL.");
                continue;
            }
            if (QuestSaveManager.Instance == null)
            {
                Debug.LogWarning("[AUTOSAVE] Skipped. QuestManager.Instance is NULL.");
                continue;
            }

            AutoSave();
        }
    }

    public void ManualSave()
    {
        string fileName = "manual_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
        bool saved = SaveGame(fileName);

        if(!saved)
            return;

        OnManualSaveCompleted?.Invoke("Manual save completed");
    }


    public void LoadGame(string saveFileName)
    {
        GameSaveData data = ReadSaveFile(saveFileName);

        if (data == null)
        {
            Debug.LogWarning("Save file not found or invalid: " + saveFileName);
            return;
        }
        
        // temporarily store the data for the load
        pendingLoadData = data;

        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene != data.sceneName)
        {
            // switch to the saved scene
            // OnSceneLoad will add saved data to the scene when it's ready
            Debug.Log("[LOAD] Loading scene from save: " + data.sceneName);
            SceneManager.LoadScene(data.sceneName);
        }
        else
        {
            RestoreGameData(pendingLoadData);
            pendingLoadData = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(scene.name == gameplaySceneName)
        {
            StartAutoSave();
        }
        else
        {
            StopAutoSave();
        }

        if (pendingLoadData == null)
            return;

        StartCoroutine(RestoreAfterSceneReady());
    }

    private IEnumerator RestoreAfterSceneReady()
    {
        yield return null;

        Debug.Log("[LOAD] Scene is ready. Restoring save data...");

        RestoreGameData(pendingLoadData);
        pendingLoadData = null;
    }

    private void WriteSaveFile(string saveFileName, GameSaveData data)
    {
        if (!Directory.Exists(savesFolderPath))
            Directory.CreateDirectory(savesFolderPath);

        string path = Path.Combine(savesFolderPath, saveFileName + ".json");
        string json = JsonUtility.ToJson(data, true);

        File.WriteAllText(path, json);

        Debug.Log("Game saved: " + path);
    }

    private GameSaveData ReadSaveFile(string saveFileName)
    {
        string path = Path.Combine(savesFolderPath, saveFileName + ".json");

        if (!File.Exists(path))
            return null;

        try
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<GameSaveData>(json);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to read save file: " + e.Message);
            return null;
        }
    }

    private void RestoreGameData(GameSaveData data)
    {
        if (HotbarManager.Instance != null && data.inventory != null)
            HotbarManager.Instance.RestoreSaveData(data.inventory);

        if (SceneObjectsManager.Instance != null && data.sceneObjects != null)
            SceneObjectsManager.Instance.RestoreSaveData(data.sceneObjects);

        if (PlayerSaveManager.Instance != null && data.player != null)
            PlayerSaveManager.Instance.RestoreSaveData(data.player);

        if(RecipeSaveManager.Instance != null && data.recipe != null)
            RecipeSaveManager.Instance.RestoreSaveData(data.recipe);

        if (QuestSaveManager.Instance != null && data.quests != null)
            QuestSaveManager.Instance.RestoreSaveData(data.quests);


        Debug.Log("Game loaded: " + data.saveName);
    }

    public void DeleteSaveFile(string saveFileName)
    {
        string path = Path.Combine(savesFolderPath, saveFileName + ".json");
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("Deleted save file: " + path);
        }
        else
        {
            Debug.LogWarning("Save file not found for deletion: " + path);
        }
    }
}