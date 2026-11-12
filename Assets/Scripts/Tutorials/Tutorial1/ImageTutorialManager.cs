using UnityEngine;
using UnityEngine.UI;

public class ImageTutorialManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private Transform contentParent;
    [SerializeField] private ImageTutorialWindow windowPrefab;
    [SerializeField] private ScrollRect scrollRect;

    [Header("Testing")]
    [SerializeField] private ImageTutorialData testTutorial;

    private CursorLockMode previousLockState;
    private bool previousCursorVisibility;

    private bool isTutorialOpen;

    private void Start()
    {
        tutorialPanel.SetActive(false);
    }

    public void ShowTutorial(ImageTutorialData tutorial)
    {
        if (tutorial == null)
        {
            Debug.LogWarning("ImageTutorialData is missing!");
            return;
        }

        if (!isTutorialOpen)
        {
            previousLockState = Cursor.lockState;
            previousCursorVisibility = Cursor.visible;
        }

        isTutorialOpen = true;

        tutorialPanel.SetActive(true);

        // Unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        ClearTutorial();

        foreach (var page in tutorial.Pages)
        {
            ImageTutorialWindow window =
                Instantiate(windowPrefab, contentParent);

            window.Initialize(page.image, page.description);
        }

        // Update UI layout
        Canvas.ForceUpdateCanvases();

        if (scrollRect != null)
        {
            scrollRect.horizontalNormalizedPosition = 0f;
        }
    }

    public void CLickTutorialCloseButton()
    {
        
    }

    public void CloseTutorial()
    {
        if (!isTutorialOpen)
            return;

        isTutorialOpen = false;

        tutorialPanel.SetActive(false);

        // Restore cursor
        Cursor.lockState = previousLockState;
        Cursor.visible = previousCursorVisibility;
    }

    private void ClearTutorial()
    {
        foreach (Transform child in contentParent)
        {
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