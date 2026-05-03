using UnityEngine;
using UnityEngine.UI;

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
