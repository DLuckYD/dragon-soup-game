using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class CutscenePlayer : MonoBehaviour
{
    [Header("Cutscene UI")]
    [SerializeField] private GameObject cutsceneRoot;

    [Header("Objects To Hide")]
    [SerializeField] private GameObject[] objectsToHideDuringCutscene;

    [Header("Components To Disable")]
    [SerializeField] private MonoBehaviour[] componentsToDisableDuringCutscene;

    [Header("Settings")]
    [SerializeField] private bool pauseGameDuringCutscene = true;
    [SerializeField] private bool hideCursorDuringCutscene = true;
    [SerializeField] private bool showCursorBeforeLoadingScene = true;

    private VideoPlayer activePlayer;
    private string sceneAfterVideo;
    private bool isPlaying = false;
    private float previousTimeScale = 1f;

    private AsyncOperation preloadOperation;
    private Coroutine preloadCoroutine;

    private void Awake()
    {
        if (cutsceneRoot != null)
            cutsceneRoot.SetActive(false);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void OnDestroy()
    {
        if (activePlayer != null)
            activePlayer.loopPointReached -= OnVideoFinished;
    }

    public void PlayAndLoadScene(VideoPlayer videoPlayer, string sceneName)
    {
        if (isPlaying)
            return;

        if (videoPlayer == null)
        {
            Debug.LogWarning("[CutscenePlayer] VideoPlayer is not assigned.");
            return;
        }

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("[CutscenePlayer] Scene name is empty.");
            return;
        }

        isPlaying = true;
        activePlayer = videoPlayer;
        sceneAfterVideo = sceneName;
        preloadOperation = null;
        AkUnitySoundEngine.SetState("Game_State", "Cutscene");

        PrepareVideoPlayer(activePlayer);
        PrepareCutsceneState();

        activePlayer.Stop();
        activePlayer.Play();

        preloadCoroutine = StartCoroutine(PreloadSceneAsync(sceneAfterVideo));

        Debug.Log($"[CutscenePlayer] Playing video: {activePlayer.name}. Next scene: {sceneAfterVideo}");
    }

    private void PrepareVideoPlayer(VideoPlayer player)
    {
        player.playOnAwake = false;
        player.isLooping = false;
        player.timeUpdateMode = VideoTimeUpdateMode.UnscaledGameTime;

        player.loopPointReached -= OnVideoFinished;
        player.loopPointReached += OnVideoFinished;
    }

    private void PrepareCutsceneState()
    {
        previousTimeScale = Time.timeScale;

        if (pauseGameDuringCutscene)
            Time.timeScale = 0f;

        if (cutsceneRoot != null)
            cutsceneRoot.SetActive(true);

        foreach (GameObject obj in objectsToHideDuringCutscene)
        {
            if (obj != null)
                obj.SetActive(false);
        }

        foreach (MonoBehaviour component in componentsToDisableDuringCutscene)
        {
            if (component != null)
                component.enabled = false;
        }

        if (hideCursorDuringCutscene)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private IEnumerator PreloadSceneAsync(string sceneName)
    {
        Debug.Log($"[CutscenePlayer] Preloading scene: {sceneName}");

        preloadOperation = SceneManager.LoadSceneAsync(sceneName);

        if (preloadOperation == null)
        {
            Debug.LogWarning($"[CutscenePlayer] Failed to preload scene: {sceneName}");
            yield break;
        }

        preloadOperation.allowSceneActivation = false;

        while (preloadOperation.progress < 0.9f)
        {
            yield return null;
        }

        Debug.Log($"[CutscenePlayer] Scene preloaded and waiting for activation: {sceneName}");
    }

    private void OnVideoFinished(VideoPlayer source)
    {
        if (source != activePlayer)
            return;

        Debug.Log("[CutscenePlayer] Video finished.");

        source.loopPointReached -= OnVideoFinished;

        StartCoroutine(LoadSceneAfterVideo());
    }

    private IEnumerator LoadSceneAfterVideo()
    {
        Time.timeScale = 1f;

        if (preloadOperation == null)
        {
            Debug.LogWarning("[CutscenePlayer] Scene was not preloaded. Loading normally.");
            SceneManager.LoadScene(sceneAfterVideo);
            yield break;
        }

        while (preloadOperation.progress < 0.9f)
        {
            yield return null;
        }

        Debug.Log($"[CutscenePlayer] Activating scene: {sceneAfterVideo}");

        preloadOperation.allowSceneActivation = true;
    }
}