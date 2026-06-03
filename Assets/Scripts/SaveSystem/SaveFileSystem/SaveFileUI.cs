using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveFileUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text fileName;
    [SerializeField] private TMP_Text fileDescription;
    [SerializeField] private Button fileButton;
    [SerializeField] private Button deleteButton;

    GameSaveData saveData;

    void Start()
    {
        fileButton.onClick.AddListener(() => GameSaveLoadManager.Instance.LoadGame(saveData.saveName));
        deleteButton.onClick.AddListener(OnDeleteButtonClicked);
    }

    public void Setup(GameSaveData data)
    {
        saveData = data;
        fileName.text = $"Save file: {data.saveName}";
        fileDescription.text = $"Date of save: {data.savedAt:yyyy-MM-dd HH:mm}";
    }

    private void OnDeleteButtonClicked()
    {
        GameSaveLoadManager.Instance.DeleteSaveFile(saveData.saveName);
        Destroy(gameObject);
    }
}
