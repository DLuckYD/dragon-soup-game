using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ListSaveFilesPanelUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private SaveFileUI saveFileItemPrefab;
    [SerializeField] private Button backToMenuButton;
    [SerializeField] private GameObject playScreenPanel;

    private string savesFolderPath;

    void Start()
    {
        backToMenuButton.onClick.AddListener(OnBackToMenuButtonClicked);
        savesFolderPath = Path.Combine(Application.persistentDataPath, "Saves");
        LoadSaveFiles();
    }

    private void OnBackToMenuButtonClicked()
    {
        gameObject.SetActive(false);
        playScreenPanel.SetActive(true);
    }

    private void LoadSaveFiles()
    {
        if (!Directory.Exists(savesFolderPath))
        {
            Debug.LogWarning($"Saves folder not found at {savesFolderPath}. No save files to display.");
            return;
        }

        List<GameSaveData> saves = GameSaveLoadManager.Instance.GetSortedSaveFiles();

        foreach (GameSaveData data in saves)
        {
            SaveFileUI item = Instantiate(saveFileItemPrefab, contentParent);
            item.Setup(data);
        }

        Debug.Log($"Loaded {saves.Count} save files from {savesFolderPath}.");
    }
}
