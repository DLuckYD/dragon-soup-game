using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementItemUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text numberText;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;

    [Header("State Visuals")]
    [SerializeField] private Image lockedOverlay;
    [SerializeField] private GameObject completedMark;

    public void Setup(AchievementDefinition achievement, Sprite icon)
    {
        numberText.text = $"{achievement.number:0}";
        titleText.text = achievement.title;
        descriptionText.text = achievement.description;

        if (icon != null)
        {
            iconImage.sprite = icon;
        }

        bool isUnlocked = achievement.unlocked;

        if (lockedOverlay != null)
        {
            lockedOverlay.enabled = !isUnlocked;
        }

        if (completedMark != null)
        {
            completedMark.SetActive(isUnlocked);
        }

        // Optional: make locked achievements visually weaker.
        iconImage.color = isUnlocked 
            ? Color.white 
            : new Color(0.45f, 0.45f, 0.45f, 1f);

        titleText.alpha = isUnlocked ? 1f : 0.6f;
        descriptionText.alpha = isUnlocked ? 1f : 0.5f;
        numberText.alpha = isUnlocked ? 1f : 0.6f;
    }
}