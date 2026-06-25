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
    [SerializeField] private GameObject saveFilesPanel;

    [SerializeField] private MainMenuUIManager mainMenuManager;
    [SerializeField] private IntroCutsceneStarter introCutsceneStarter;

    private void Start()
    {
        loadGameButton.onClick.AddListener(LoadSaveFile);
        continueButton.onClick.AddListener(ContinueLastGame);
        newGameButton.onClick.AddListener(CreateNewGame);
        backButton.onClick.AddListener(OnBackPressed);
    }

    public void LoadSaveFile()
    {
        WwiseAudioManager.Instance.PostEvent("Ui_Button_Clicked", gameObject);
        gameObject.SetActive(false);
        saveFilesPanel.SetActive(true);
    }

    public void ContinueLastGame()
    {
        WwiseAudioManager.Instance.PostEvent("Ui_Button_Clicked", gameObject);
        GameSaveLoadManager.Instance.ContinueGame();
    }

    public void CreateNewGame()
    {
        WwiseAudioManager.Instance.PostEvent("Ui_Button_Clicked", gameObject);

        GameSaveLoadManager.Instance.PrepareNewGame();

        if (introCutsceneStarter != null)
        {
            introCutsceneStarter.PlayIntro();
        }
        else
        {
            Debug.LogWarning("[MainMenu] IntroCutsceneStarter is not assigned. Loading SampleScene directly.");
            GameSaveLoadManager.Instance.StartNewGame();
        }
    }

    public void OnBackPressed()
    {
        WwiseAudioManager.Instance.PostEvent("Ui_Button_Clicked", gameObject);
        mainMenuManager.ShowStartScreen();
    }
}
