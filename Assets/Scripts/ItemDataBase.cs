using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Inventory/Item Database")]
public class ItemDataBase : ScriptableObject
{
    [Header("Items")]
    [SerializeField] private List<ItemData> items = new List<ItemData>();

#if UNITY_EDITOR
    [Header("Editor Auto Fill")]
    [SerializeField] private string itemDataFolderPath = "Assets";

    [ContextMenu("Rebuild Item Database")]
    private void RebuildDatabase()
    {
        items.Clear();

        string[] guids = AssetDatabase.FindAssets("t:ItemData", new[] { itemDataFolderPath });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ItemData item = AssetDatabase.LoadAssetAtPath<ItemData>(path);

            if (item != null && !items.Contains(item))
            {
                items.Add(item);
            }
        }

        items.Sort((a, b) => a.id.CompareTo(b.id));

        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();

        Debug.Log("Item Database rebuilt. Items found: " + items.Count);
    }
#endif

    public ItemData GetItemById(string id)
    {
        foreach (ItemData item in items)
        {
            if (item.id.Equals(id))
                return item;
        }

        Debug.LogWarning("Item not found with id: " + id);
        return null;
    }

    public List<ItemData> GetAllItems()
    {
        return items;
    }
}