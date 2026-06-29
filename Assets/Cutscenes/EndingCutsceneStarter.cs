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

    [Header("Ending Logic")]
    [SerializeField] private bool badEndingIfDarkEntityWasUsed = true;

    [Header("Scene")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private void Awake()
    {
        if (cutscenePlayer == null)
            cutscenePlayer = FindObjectOfType<CutscenePlayer>();

        if (darkEntity == null)
            darkEntity = FindObjectOfType<DarkEntity>();
    }

    public void PlayEnding()
    {
        if (cutscenePlayer == null)
        {
            Debug.LogWarning("[EndingCutsceneStarter] CutscenePlayer is not assigned.");
            return;
        }

        VideoPlayer selectedEnding = GetSelectedEndingPlayer();

        if (selectedEnding == null)
        {
            Debug.LogWarning("[EndingCutsceneStarter] Selected ending VideoPlayer is not assigned.");
            return;
        }

        cutscenePlayer.PlayAndLoadScene(selectedEnding, mainMenuSceneName);
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
        bool darkEntityWasUsed = (usageCount >= ((recipeCount/2) +1));

        if (badEndingIfDarkEntityWasUsed)
            return darkEntityWasUsed ? badEndingPlayer : goodEndingPlayer;

        return darkEntityWasUsed ? goodEndingPlayer : badEndingPlayer;
    }
}
