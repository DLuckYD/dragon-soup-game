using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class EndingCutsceneStarter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CutscenePlayer cutscenePlayer;
    [SerializeField] private DarkEntity darkEntity;
    [SerializeField] private RecipeDatabase recipeDatabase;

    [Header("Ending Videos")]
    [SerializeField] private VideoPlayer badEndingPlayer;
    [SerializeField] private VideoPlayer goodEndingPlayer;

    [Header("Fade")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeOutDuration = 1f;
    [SerializeField] private float fadeInDuration = 0.5f;

    [Header("Ending Logic")]
    [SerializeField] private bool badEndingIfDarkEntityWasUsed = true;

    [Header("Scene")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool isPlayingEnding = false;

    private void Awake()
    {
        if (cutscenePlayer == null)
            cutscenePlayer = FindObjectOfType<CutscenePlayer>();

        if (darkEntity == null)
            darkEntity = FindObjectOfType<DarkEntity>();

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.gameObject.SetActive(false);
        }
    }

    public void PlayEnding()
    {
        if (isPlayingEnding)
            return;

        StartCoroutine(PlayEndingRoutine());
    }

    private IEnumerator PlayEndingRoutine()
    {
        isPlayingEnding = true;

        if (cutscenePlayer == null)
        {
            Debug.LogWarning("[EndingCutsceneStarter] CutscenePlayer is not assigned.");
            isPlayingEnding = false;
            yield break;
        }

        VideoPlayer selectedEnding = GetSelectedEndingPlayer();

        if (selectedEnding == null)
        {
            Debug.LogWarning("[EndingCutsceneStarter] Selected ending VideoPlayer is not assigned.");
            isPlayingEnding = false;
            yield break;
        }

        yield return FadeToBlack();
        fadeCanvasGroup.gameObject.SetActive(false);

        cutscenePlayer.PlayAndLoadScene(selectedEnding, mainMenuSceneName);

    }

    private IEnumerator FadeToBlack()
    {
        if (fadeCanvasGroup == null)
            yield break;

        fadeCanvasGroup.gameObject.SetActive(true);
        fadeCanvasGroup.blocksRaycasts = true;

        float timer = 0f;

        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeOutDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = 1f;
    }

    private IEnumerator FadeFromBlack()
    {
        if (fadeCanvasGroup == null)
            yield break;

        float timer = 0f;

        while (timer < fadeInDuration)
        {
            timer += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeInDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;
        fadeCanvasGroup.gameObject.SetActive(false);
    }

    private VideoPlayer GetSelectedEndingPlayer()
    {
        int usageCount = 0;

        if (darkEntity != null)
        {
            usageCount = darkEntity.DarkEntityUsageCount;
        }
        else
        {
            Debug.LogWarning("[EndingCutsceneStarter] DarkEntity not found. Good ending will be used by default.");
        }

        int recipeCount = recipeDatabase.GetAllRecipes().Count;
        bool darkEntityWasUsed = usageCount >= ((recipeCount / 2) + 1);

        if (badEndingIfDarkEntityWasUsed)
            return darkEntityWasUsed ? badEndingPlayer : goodEndingPlayer;

        return darkEntityWasUsed ? goodEndingPlayer : badEndingPlayer;
    }
}