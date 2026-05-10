using System.Linq;
using UnityEngine;

public class AchievementsPanelUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private AchievementItemUI achievementItemPrefab;

    [Header("Resources Paths")]
    [SerializeField] private string achievementsJsonPath = "Achievements/achievements";
    [SerializeField] private string iconsFolderPath = "AchievementIcons";

    [Header("Fallback")]
    [SerializeField] private Sprite defaultIcon;

    private void OnEnable()
    {
        BuildAchievementsUI();
    }

    private void BuildAchievementsUI()
    {
        ClearOldItems();

        TextAsset jsonFile = Resources.Load<TextAsset>(achievementsJsonPath);

        if (jsonFile == null)
        {
            Debug.LogError($"[ACHIEVEMENTS] JSON file not found at Resources/{achievementsJsonPath}.json");
            return;
        }

        AchievementDefinitionList achievementList =
            JsonUtility.FromJson<AchievementDefinitionList>(jsonFile.text);

        if (achievementList == null || achievementList.achievements == null)
        {
            Debug.LogError("[ACHIEVEMENTS] Failed to parse achievements JSON.");
            return;
        }

        foreach (AchievementDefinition achievement in achievementList.achievements.OrderBy(a => a.number))
        {
            AchievementItemUI item = Instantiate(achievementItemPrefab, contentParent);

            Sprite icon = Resources.Load<Sprite>($"{iconsFolderPath}/{achievement.iconId}");

            if (icon == null)
            {
                Debug.LogWarning($"[ACHIEVEMENTS] Icon not found: {iconsFolderPath}/{achievement.iconId}");
                icon = defaultIcon;
            }

            item.Setup(achievement, icon);
        }

        Debug.Log($"[ACHIEVEMENTS] Loaded {achievementList.achievements.Count} achievements.");
    }

    private void ClearOldItems()
    {
        for (int i = contentParent.childCount - 1; i >= 0; i--)
        {
            Destroy(contentParent.GetChild(i).gameObject);
        }
    }
}