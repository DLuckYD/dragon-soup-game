using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button audioButton;
    [SerializeField] private Button videoButton;
    [SerializeField] private Button inputButton;
    [SerializeField] private Button othersButton;
    [SerializeField] private Button backButton;

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
        backButton.onClick.AddListener(OnBackPressed);

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
        HideAllPanels();
        ResetButtons();
        audioPanel.SetActive(true);
        audioButton.interactable = false;

        Debug.Log("ShowAudioPanel called");
    }

    public void ShowVideoPanel()
    {
        HideAllPanels();
        ResetButtons();
        videoPanel.SetActive(true);
        videoButton.interactable = false;
    }

    public void ShowInputPanel()
    {
        HideAllPanels();
        ResetButtons();
        inputPanel.SetActive(true);
        inputButton.interactable = false;
    }

    public void ShowOthersPanel()
    {
        HideAllPanels();
        ResetButtons();
        othersPanel.SetActive(true);
        othersButton.interactable = false;
    }
    public void OnBackPressed()
    {
        mainMenuManager.ShowStartScreen();
    }
}

[System.Serializable]
public class SettingsData
{
    public float masterVolume;
    public float musicVolume;
    public float effectsVolume;

    public int resolutionWidth;
    public int resolutionHeight;
    public int screenModeIndex;
}