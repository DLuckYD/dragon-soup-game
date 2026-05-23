using UnityEngine;

[CreateAssetMenu(fileName = "AdventurerData", menuName = "Scriptable Objects/AdventurerData")]
public class AdventurerData : ScriptableObject
{
    [Header("Identity")]
    public string id;
    public string displayName;
    public string classType;

    [Header("Legacy Visual")]
    public Sprite skinSprite; // Legacy logic. Can be removed later.

    [Header("Modular Visual")]
    public Sprite faceSprite;
    public Sprite bodySprite;
    public Sprite weaponSprite;

    [Header("Reward Preferences")]
    public string preferredRewardItemId;
    public ItemState preferredItemState = ItemState.None;

    [Header("Balancing")]
    public int difficultyModifier = 0;

    [Header("Future Systems")]
    public string viceId;

    public void GetAdventurerData()
    {
        Debug.Log(
            $"Adventurer id: {id}, name: {displayName}, classType: {classType}, " +
            $"preferred item: {preferredRewardItemId}, preferred state: {preferredItemState}, " +
            $"difficulty modifier: {difficultyModifier}"
        );
    }
}