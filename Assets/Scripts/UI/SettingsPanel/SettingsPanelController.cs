using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class SettingsData
{
    public float masterVolume = 1f;
    public float musicVolume = 1f;
    public float effectsVolume = 1f;

    public int resolutionWidth = 2560;
    public int resolutionHeight = 1440;
    public int screenModeIndex = 1;

    public string inputBindingOverridesJson = "";
    public float mouseSensitivity = 1f;

    public int autoSaveTime = 5;
}

public class SettingsPanelController: MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button audioButton;
    [SerializeField] private Button videoButton;
    [SerializeField] private Button inputButton;
    [SerializeField] private Button othersButton;
    [SerializeField] private Button backToMenuButton;

    [Header("Panels")]
    [SerializeField] private GameObject audioPanel;
    [SerializeField] private GameObject videoPanel;
    [SerializeField] private GameObject inputPanel;
    [SerializeField] private GameObject othersPanel;

    [SerializeField] private MainMenuUIManager mainMenuManager;

    private void Start()
    {
        audioButton.onClick.AddListener(ShowAudioPanel);
        videoButton.onClick.AddListener(ShowVideoPanel);
        inputButton.onClick.AddListener(ShowInputPanel);
        othersButton.onClick.AddListener(ShowOthersPanel);
        backToMenuButton.onClick.AddListener(BackToMenu);

        ShowAudioPanel();
    }

    private void ResetButtons()
    {
        audioButton.interactable = true;
        videoButton.interactable = true;
        inputButton.interactable = true;
        othersButton.interactable = true;
    }

    private void HideAllPanels()
    {
        audioPanel.SetActive(false);
        videoPanel.SetActive(false);
        inputPanel.SetActive(false);
        othersPanel.SetActive(false);
    }

    public void ShowAudioPanel()
    {
        WwiseAudioManager.Instance.PostEvent("Ui_Button_Clicked", gameObject);
        HideAllPanels();
        ResetButtons();
        audioPanel.SetActive(true);
        audioButton.interactable = false;

        Debug.Log("ShowAudioPanel called");
    }

    public void ShowVideoPanel()
    {
        WwiseAudioManager.Instance.PostEvent("Ui_Button_Clicked", gameObject);
        HideAllPanels();
        ResetButtons();
        videoPanel.SetActive(true);
        videoButton.interactable = false;
    }

    public void ShowInputPanel()
    {
        WwiseAudioManager.Instance.PostEvent("Ui_Button_Clicked", gameObject);
        HideAllPanels();
        ResetButtons();
        inputPanel.SetActive(true);
        inputButton.interactable = false;
    }

    public void ShowOthersPanel()
    {
        WwiseAudioManager.Instance.PostEvent("Ui_Button_Clicked", gameObject);
        HideAllPanels();
        ResetButtons();
        othersPanel.SetActive(true);
        othersButton.interactable = false;
    }
    public void BackToMenu()
    {
        WwiseAudioManager.Instance.PostEvent("Ui_Button_Clicked", gameObject);
        SettingsSaveLoadManager.Instance.SaveSettings();
        this.gameObject.SetActive(false);

        mainMenuManager.ShowStartScreen();
    }
}