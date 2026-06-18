using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class CutsceneController : MonoBehaviour
{
    private enum CutsceneEndAction
    {
        ShowFinalPanel,
        LoadScene
    }

    [Header("Video")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private GameObject videoPanel;

    [Header("After Video")]
    [SerializeField] private CutsceneEndAction endAction = CutsceneEndAction.ShowFinalPanel;
    [SerializeField] private GameObject finalPanel;
    [SerializeField] private string sceneToLoad;

    [Header("Settings")]
    [SerializeField] private bool playOnStart = false;
    [SerializeField] private bool unlockCursorAfterVideo = true;

    private bool cutsceneStarted = false;
    private Action onCutsceneFinished;

    private void Awake()
    {
        Time.timeScale = 1f;

        if (finalPanel != null)
            finalPanel.SetActive(false);

        if (videoPanel != null)
            videoPanel.SetActive(false);

        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.loopPointReached += OnVideoFinished;
        }
    }

    private void Start()
    {
        if (playOnStart)
        {
            PlayCutscene();
        }
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }

    public void PlayCutscene(Action onFinished = null)
    {
        if (cutsceneStarted)
            return;

        cutsceneStarted = true;
        onCutsceneFinished = onFinished;

        if (videoPlayer == null)
        {
            Debug.LogWarning("[CutsceneController] VideoPlayer is not assigned.");
            FinishCutscene();
            return;
        }

        if (videoPanel != null)
            videoPanel.SetActive(true);

        if (finalPanel != null)
            finalPanel.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        videoPlayer.Stop();
        videoPlayer.Play();

        Debug.Log("[CutsceneController] Cutscene started.");
    }

    private void OnVideoFinished(VideoPlayer source)
    {
        Debug.Log("[CutsceneController] Cutscene finished.");

        FinishCutscene();
    }

    private void FinishCutscene()
    {
        if (onCutsceneFinished != null)
        {
            Action callback = onCutsceneFinished;
            onCutsceneFinished = null;

            callback.Invoke();
            return;
        }

        if (unlockCursorAfterVideo)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        if (endAction == CutsceneEndAction.ShowFinalPanel)
        {
            if (videoPanel != null)
                videoPanel.SetActive(false);

            ShowFinalPanel();
        }
        else if (endAction == CutsceneEndAction.LoadScene)
        {
            LoadNextScene();
        }
    }

    private void ShowFinalPanel()
    {
        if (finalPanel != null)
        {
            finalPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("[CutsceneController] Final panel is not assigned.");
        }
    }

    private void LoadNextScene()
    {
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogWarning("[CutsceneController] Scene name is empty.");
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneToLoad);
    }
}