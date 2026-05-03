using UnityEngine;

using System;
using System.IO;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameSaveLoadManager : MonoBehaviour
{
    public static GameSaveLoadManager Instance { get; private set; }

    private string savesFolderPath;

    // used to store loaded data temporarily until the scene is fully loaded and ready to apply it
    private GameSaveData pendingLoadData;

    [Header("Autosave")]
    [SerializeField] private float autoSaveIntervalSeconds = 360f; // 6 minutes
    [SerializeField] private string gameplaySceneName = "SampleScene";

    private Coroutine autoSaveCoroutine;

    public static event Action<string> OnAutoSaveCompleted;
    public static event Action<string> OnManualSaveCompleted;

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

    public void StartNewGame()
    {
        pendingLoadData = null;
        SceneManager.LoadScene("SampleScene");
    }

    public void ContinueGame()
    {
        LoadGame("autosave");
    }

    public void SaveGame(string saveFileName)
    {
        GameSaveData data = new GameSaveData();

        data.version = 1;
        data.saveName = saveFileName;
        data.sceneName = SceneManager.GetActiveScene().name;
        data.savedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        if (HotbarManager.Instance != null)
        {
            data.inventory = HotbarManager.Instance.CaptureSaveData();
        }

        WriteSaveFile(saveFileName, data);
    }

    public void AutoSave()
    {
        SaveGame("autosave");
        OnAutoSaveCompleted?.Invoke("Autosave completed");
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
            AutoSave();
        }
    }

    public void ManualSave()
    {
        string fileName = "manual_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
        SaveGame(fileName);
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

        Debug.Log("[LOAD] Scene loaded. Restoring save data...");

        // apply the loaded data to the scene
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


        Debug.Log("Game loaded: " + data.saveName);
    }
}