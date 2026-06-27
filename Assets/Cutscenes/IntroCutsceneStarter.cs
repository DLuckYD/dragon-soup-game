using UnityEngine;
using UnityEngine.Video;

public class IntroCutsceneStarter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CutscenePlayer cutscenePlayer;
    [SerializeField] private VideoPlayer introPlayer;

    [Header("Scene")]
    [SerializeField] private string gameplaySceneName = "SampleScene";

    private void Awake()
    {
        if (cutscenePlayer == null)
            cutscenePlayer = FindObjectOfType<CutscenePlayer>();
    }

    public void PlayIntro()
    {
        if (cutscenePlayer == null)
        {
            Debug.LogWarning("[IntroCutsceneStarter] CutscenePlayer is not assigned.");
            return;
        }

        if (introPlayer == null)
        {
            Debug.LogWarning("[IntroCutsceneStarter] Intro VideoPlayer is not assigned.");
            return;
        }

        cutscenePlayer.PlayAndLoadScene(introPlayer, gameplaySceneName);
    }
}