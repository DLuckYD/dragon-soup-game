using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private NavigationNotificationUI notificationUI;

    private HashSet<TutorialID> completedTutorials = new();

    private TutorialID? activeTutorial;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        PlayerInteraction.OnInteraction += ShowInteractionTutorial;
        PlayerInteraction.OnEndedInteraction += HideTutorial;
    }

    private void OnDisable()
    {
        PlayerInteraction.OnInteraction -= ShowInteractionTutorial;
        PlayerInteraction.OnEndedInteraction -= HideTutorial;
    }

    private void Start()
    {
        if (!IsCompleted(TutorialID.Movement))
        {
            ShowTutorial(TutorialID.Movement, "Use", "WASD", "to move");
        }
        else if (!IsCompleted(TutorialID.CameraLook))
        {
            ShowTutorial(TutorialID.CameraLook, "Move", "Mouse", "to look around");
        }
    }

    public bool IsCompleted(TutorialID id)
    {
        return completedTutorials.Contains(id);
    }

    // New method with three separate text fields
    public void ShowTutorial(
        TutorialID id,
        string prefix,
        string key,
        string action)
    {
        if (IsCompleted(id))
            return;

        if (activeTutorial.HasValue && activeTutorial.Value != id)
            return;

        activeTutorial = id;

        notificationUI.ShowMessage(prefix, key, action);
    }

    // Receives interaction events
    private void ShowInteractionTutorial(TutorialID id, string prefix, string key, string message)
    {
        // Temporary compatibility with existing PlayerInteraction
        ShowTutorial(id, prefix, key, message);
    }

    public void HideTutorial(TutorialID id)
    {
        if (activeTutorial != id)
            return;

        activeTutorial = null;
        notificationUI.HideMessage();
    }

    public void CompleteTutorial(TutorialID id)
    {
        if (!completedTutorials.Add(id))
            return;

        HideTutorial(id);

        Debug.Log($"[TUTORIAL] Completed: {id}");

        if (id == TutorialID.Movement)
        {
            ShowTutorial(
                TutorialID.CameraLook,
                "Move",
                "Mouse",
                "to look around"
            );
        }
    }
}