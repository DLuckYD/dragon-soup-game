using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
public class ItemData : ScriptableObject
{
    public string id;
    public string displayName;
    public Sprite icon;

    [Header("Stack")]
    public bool isStackable;
    public int maxStackSize = 1;

    [Header("World")]
    public GameObject worldPrefab;
}
//add test comment 
