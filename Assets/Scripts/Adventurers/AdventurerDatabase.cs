using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AdventurerDatabase", menuName = "Dragon Soup/Adventurers/Adventurer Database")]
public class AdventurerDatabase : ScriptableObject
{
    public List<AdventurerData> adventurers = new List<AdventurerData>();

    public AdventurerData GetRandom()
    {
        if (adventurers == null || adventurers.Count == 0)
        {
            Debug.LogWarning("[ADVENTURER DATABASE] No adventurers available.");
            return null;
        }

        int index = Random.Range(0, adventurers.Count);
        return adventurers[index];
    }

    public AdventurerData GetById(string id)
    {
        if (string.IsNullOrWhiteSpace(id) || adventurers == null)
            return null;

        foreach (AdventurerData adventurer in adventurers)
        {
            if (adventurer == null)
                continue;

            if (adventurer.id == id)
                return adventurer;
        }

        return null;
    }
}