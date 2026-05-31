using System.IO;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PlayScreenPanelController : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button loadGameButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button backButton;

    [SerializeField] private MainMenuUIManager mainMenuManager;

    private void Start()
    {
        loadGameButton.onClick.AddListener(LoadSaveFile);
        continueButton.onClick.AddListener(ContinueLastGame);
        newGameButton.onClick.AddListener(CreateNewGame);
        backButton.onClick.AddListener(OnBackPressed);
    }

    public void LoadSaveFile()
    {
#if UNITY_EDITOR
        string path = EditorUtility.OpenFilePanel(
            "Select Save File",
            Path.Combine(Application.persistentDataPath, "Saves"),
            "json"
        );

        if (!string.IsNullOrEmpty(path))
        {
            GameSaveLoadManager.Instance.LoadTheSaveFile(path);
        }
#else
    Debug.LogWarning("OpenFilePanel works only in Unity Editor. Use in-game save list for builds.");
#endif
    }

    public void ContinueLastGame()
    {
        GameSaveLoadManager.Instance.ContinueGame();
    }

    public void CreateNewGame()
    {
        GameSaveLoadManager.Instance.StartNewGame();
    }

    public void OnBackPressed()
    {
        mainMenuManager.ShowStartScreen();
    }
}
