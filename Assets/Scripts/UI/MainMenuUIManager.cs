using UnityEngine;
using UnityEngine.UI;

public class MainMenuUIManager : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button achievementsButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button quitGameButton;

    [Header("Panels")]
    [SerializeField] private GameObject startScreenPanel;
    [SerializeField] private GameObject playScreenPanel;
    [SerializeField] private GameObject settingsScreenPanel;
    [SerializeField] private GameObject achievementsScreenPanel;
    [SerializeField] private GameObject creditsScreenPanel;

    private void Start()
    {
        playButton.onClick.AddListener(ShowPlayScreen);
        settingsButton.onClick.AddListener(ShowSettingsScreen);
        achievementsButton.onClick.AddListener(ShowAchievementsScreen);
        creditsButton.onClick.AddListener(ShowCreditsScreen);
        quitGameButton.onClick.AddListener(QuitGame);

        ShowStartScreen();
    }

    private void HideAllScreens()
    {
        startScreenPanel.SetActive(false);
        playScreenPanel.SetActive(false);
        settingsScreenPanel.SetActive(false);
        achievementsScreenPanel.SetActive(false);
        creditsScreenPanel.SetActive(false);
    }

    public void ShowStartScreen()
    {
        HideAllScreens();
        startScreenPanel.SetActive(true);
    }

    public void ShowPlayScreen()
    {
        WwiseAudioManager.Instance.PostEvent("Ui_Button_Clicked", gameObject);
        HideAllScreens();
        playScreenPanel.SetActive(true);
    }

    public void ShowSettingsScreen()
    {
        WwiseAudioManager.Instance.PostEvent("Ui_Button_Clicked", gameObject);
        HideAllScreens();
        settingsScreenPanel.SetActive(true);
    }

    public void ShowAchievementsScreen()
    {
        WwiseAudioManager.Instance.PostEvent("Ui_Button_Clicked", gameObject);
        HideAllScreens();
        achievementsScreenPanel.SetActive(true);
    }

    public void ShowCreditsScreen()
    {
        WwiseAudioManager.Instance.PostEvent("Ui_Button_Clicked", gameObject);
        HideAllScreens();
        creditsScreenPanel.SetActive(true);
    }

    public void QuitGame()
    {
        WwiseAudioManager.Instance.PostEvent("Ui_Button_Cancel", gameObject);
        Debug.Log("Quit Game");
        Application.Quit();
    }
}