using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class AdventurerCsvImporter
{
    private const string AdventurerOutputFolder = "Assets/Data/Adventurers/Generated";
    private const string AdventurerDatabasePath = "Assets/Data/Adventurers/AdventurerDatabase.asset";

    [MenuItem("Tools/Dragon Soup/Import Adventurers From CSV")]
    public static void ImportAdventurersFromCsv()
    {
        string csvPath = EditorUtility.OpenFilePanel(
            "Select Adventurers CSV",
            Application.dataPath,
            "csv"
        );

        if (string.IsNullOrEmpty(csvPath))
        {
            Debug.Log("[ADVENTURER IMPORT] Import cancelled.");
            return;
        }

        EnsureFolderExists("Assets/Data");
        EnsureFolderExists("Assets/Data/Adventurers");
        EnsureFolderExists(AdventurerOutputFolder);

        List<Dictionary<string, string>> rows = ReadCsv(csvPath);

        if (rows.Count == 0)
        {
            Debug.LogWarning("[ADVENTURER IMPORT] CSV has no data rows.");
            return;
        }

        AdventurerDatabase database = GetOrCreateDatabase();
        database.adventurers.Clear();

        int createdCount = 0;
        int updatedCount = 0;
        int failedCount = 0;

        foreach (Dictionary<string, string> row in rows)
        {
            string id = GetValue(row, "id", "adventurer_id", "npc_id");

            if (string.IsNullOrWhiteSpace(id))
            {
                Debug.LogWarning("[ADVENTURER IMPORT] Skipped row because id is missing.");
                failedCount++;
                continue;
            }

            string safeAssetName = MakeSafeAssetName(id);
            string assetPath = $"{AdventurerOutputFolder}/{safeAssetName}.asset";

            AdventurerData adventurer = AssetDatabase.LoadAssetAtPath<AdventurerData>(assetPath);

            bool wasCreated = false;

            if (adventurer == null)
            {
                adventurer = ScriptableObject.CreateInstance<AdventurerData>();
                AssetDatabase.CreateAsset(adventurer, assetPath);
                wasCreated = true;
            }

            FillAdventurerData(adventurer, row);

            EditorUtility.SetDirty(adventurer);

            if (!database.adventurers.Contains(adventurer))
                database.adventurers.Add(adventurer);

            if (wasCreated)
                createdCount++;
            else
                updatedCount++;
        }

        EditorUtility.SetDirty(database);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            $"[ADVENTURER IMPORT] Finished.\n" +
            $"Created: {createdCount}\n" +
            $"Updated: {updatedCount}\n" +
            $"Failed: {failedCount}\n" +
            $"Database: {AdventurerDatabasePath}"
        );
    }

    private static void FillAdventurerData(AdventurerData adventurer, Dictionary<string, string> row)
    {
        adventurer.id = GetValue(row, "id", "adventurer_id", "npc_id");
        adventurer.displayName = GetValue(row, "displayName", "display_name", "npc_name");
        adventurer.classType = GetValue(row, "classType", "class_type", "npc_class");

        adventurer.preferredRewardItemId = GetValue(
            row,
            "preferredRewardItemId",
            "preferred_reward_item_id",
            "reward_item_id",
            "npc_reward_preference"
        );

        string stateText = GetValue(
            row,
            "preferredItemState",
            "preferred_item_state",
            "preferred_upgrade_state",
            "npc_upgrade_preference"
        );

        adventurer.preferredItemState = ParseItemState(stateText);

        string faceSpriteId = GetValue(row, "faceSpriteId", "face_sprite_id", "face_id", "npc_face");
        string bodySpriteId = GetValue(row, "bodySpriteId", "body_sprite_id", "body_id", "npc_body");
        string weaponSpriteId = GetValue(row, "weaponSpriteId", "weapon_sprite_id", "weapon_id", "npc_weapon");

        adventurer.faceSprite = FindSpriteByName(faceSpriteId);
        adventurer.bodySprite = FindSpriteByName(bodySpriteId);
        adventurer.weaponSprite = FindSpriteByName(weaponSpriteId);

        adventurer.viceId = GetValue(row, "viceId", "vice_id", "personality_id");

        string difficultyModifierText = GetValue(row, "difficultyModifier", "difficulty_modifier");

        if (int.TryParse(difficultyModifierText, out int difficultyModifier))
            adventurer.difficultyModifier = difficultyModifier;
    }

    private static ItemState ParseItemState(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ItemState.None;

        if (Enum.TryParse(value.Trim(), true, out ItemState parsedState))
            return parsedState;

        Debug.LogWarning($"[ADVENTURER IMPORT] Unknown ItemState '{value}'. Using None.");
        return ItemState.None;
    }

    private static Sprite FindSpriteByName(string spriteName)
    {
        if (string.IsNullOrWhiteSpace(spriteName))
            return null;

        string[] guids = AssetDatabase.FindAssets($"{spriteName} t:Sprite");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

            if (sprite != null && sprite.name == spriteName)
                return sprite;
        }

        Debug.LogWarning($"[ADVENTURER IMPORT] Sprite not found: {spriteName}");
        return null;
    }

    private static AdventurerDatabase GetOrCreateDatabase()
    {
        AdventurerDatabase database = AssetDatabase.LoadAssetAtPath<AdventurerDatabase>(AdventurerDatabasePath);

        if (database != null)
            return database;

        EnsureFolderExists("Assets/Data");
        EnsureFolderExists("Assets/Data/Adventurers");

        database = ScriptableObject.CreateInstance<AdventurerDatabase>();
        AssetDatabase.CreateAsset(database, AdventurerDatabasePath);

        return database;
    }

    private static List<Dictionary<string, string>> ReadCsv(string csvPath)
    {
        List<Dictionary<string, string>> rows = new List<Dictionary<string, string>>();

        string[] lines = File.ReadAllLines(csvPath);

        if (lines.Length <= 1)
            return rows;

        List<string> headers = ParseCsvLine(lines[0]);

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
                continue;

            List<string> values = ParseCsvLine(lines[i]);
            Dictionary<string, string> row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            for (int j = 0; j < headers.Count; j++)
            {
                string header = headers[j].Trim();
                string value = j < values.Count ? values[j].Trim() : "";

                row[header] = value;
            }

            rows.Add(row);
        }

        return rows;
    }

    private static List<string> ParseCsvLine(string line)
    {
        List<string> result = new List<string>();

        bool insideQuotes = false;
        string current = "";

        for (int i = 0; i < line.Length; i++)
        {
            char character = line[i];

            if (character == '"')
            {
                insideQuotes = !insideQuotes;
                continue;
            }

            if (character == ',' && !insideQuotes)
            {
                result.Add(current);
                current = "";
                continue;
            }

            current += character;
        }

        result.Add(current);

        return result;
    }

    private static string GetValue(Dictionary<string, string> row, params string[] possibleKeys)
    {
        foreach (string key in possibleKeys)
        {
            if (row.TryGetValue(key, out string value))
                return value;
        }

        return "";
    }

    private static string MakeSafeAssetName(string rawName)
    {
        string safeName = rawName.Trim();

        foreach (char invalidChar in Path.GetInvalidFileNameChars())
        {
            safeName = safeName.Replace(invalidChar.ToString(), "");
        }

        safeName = safeName.Replace(" ", "_");

        return safeName;
    }

    private static void EnsureFolderExists(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath))
            return;

        string parentFolder = Path.GetDirectoryName(folderPath)?.Replace("\\", "/");
        string folderName = Path.GetFileName(folderPath);

        if (string.IsNullOrEmpty(parentFolder) || string.IsNullOrEmpty(folderName))
            return;

        if (!AssetDatabase.IsValidFolder(parentFolder))
            EnsureFolderExists(parentFolder);

        AssetDatabase.CreateFolder(parentFolder, folderName);
    }
}