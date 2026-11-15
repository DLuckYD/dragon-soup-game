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
    AddIngredient,
    UpgradeItem,
    TalkToAdventurerToGiveQuest,
    TalkToAdventurerToGetRequest,
    GiveItemToAdventurer,
    TalkToDarkEntity,
    
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

        if (DynamicTutorialManager.Instance.IsCompleted(tutorialID))
            return;

        DynamicTutorialManager.Instance?.ShowTutorial(
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

        DynamicTutorialManager.Instance.HideTutorial(tutorialID);
    }
}
