using UnityEngine;
using UnityEngine.UI;

public class VideoTutorialManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private Transform contentParent;
    [SerializeField] private VideoTutorialPageUI windowPrefab;
    [SerializeField] private ScrollRect scrollRect;

    [Header("Testing")]
    [SerializeField] private VideoTutorialData testTutorial;

    private CursorLockMode previousLockState;
    private bool previousCursorVisibility;

    private bool isTutorialOpen;

    private void Start()
    {
        tutorialPanel.SetActive(false);
    }

    public void ShowTutorial(VideoTutorialData  tutorial)
    {
        if (tutorial == null)
        {
            Debug.LogWarning("VideoTutorialData is missing!");
            return;
        }

        if (!isTutorialOpen)
        {
            previousLockState = Cursor.lockState;
            previousCursorVisibility = Cursor.visible;
        }

        isTutorialOpen = true;

        tutorialPanel.SetActive(true);
        Time.timeScale = 0f;

        // Unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        ClearTutorial();

        foreach (var page in tutorial.Pages)
        {
            VideoTutorialPageUI window =
                Instantiate(windowPrefab, contentParent);

            window.Initialize(page);
        }

        // Update UI layout
        Canvas.ForceUpdateCanvases();

        if (scrollRect != null)
        {
            scrollRect.horizontalNormalizedPosition = 0f;
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }

    public void ClickTutorialCloseButton()
    {
        CloseTutorial();
    }

    public void CloseTutorial()
    {
        if (!isTutorialOpen)
            return;

        isTutorialOpen = false;

        // Stop all videos
        foreach (Transform child in contentParent)
        {
            VideoTutorialPageUI page =
                child.GetComponent<VideoTutorialPageUI>();

            if (page != null)
                page.Stop();
        }

        tutorialPanel.SetActive(false);
        Time.timeScale = 1f;

        // Restore cursor
        Cursor.lockState = previousLockState;
        Cursor.visible = previousCursorVisibility;
    }

    private void ClearTutorial()
    {
        foreach (Transform child in contentParent)
        {
            VideoTutorialPageUI page =
                child.GetComponent<VideoTutorialPageUI>();

            if (page != null)
                page.Stop();

            Destroy(child.gameObject);
        }
    }

    // Temporary testing
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (isTutorialOpen)
            {
                CloseTutorial();
            }
            else
            {
                ShowTutorial(testTutorial);
            }
        }
    }
}