using UnityEngine;

public enum TutorialID
{
    Movement,
    CameraLook,
    ItemPickUp,
    ItemDrop,
    AddToInventory,
    DropFromInventory,
    OpenCookBook,
    ChooseRecipe,
    UpgradeItem,
    TalkToAdventurer,
    TalkToDarkEntity,
    GiveItemToAdventurer
}
public class TutorialTrigger : MonoBehaviour
{
    [SerializeField] private TutorialID tutorialID;
    [SerializeField] private string tutorialPreffix;
    [SerializeField] private string tutorialKey;
    [SerializeField] private string tutorialMessage;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (TutorialManager.Instance.IsCompleted(tutorialID))
            return;

        TutorialManager.Instance?.ShowTutorial(
            tutorialID,
            tutorialPreffix,
            tutorialKey,
            tutorialMessage
        );
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        TutorialManager.Instance.HideTutorial(tutorialID);
    }
}
