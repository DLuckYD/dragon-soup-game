using UnityEngine;
using UnityEngine.TextCore.Text;

[CreateAssetMenu(fileName = "AdventurerData", menuName = "Scriptable Objects/AdventurerData")]
public class AdventurerData : ScriptableObject
{
    public string id;
    public string displayName;
    public Sprite skinSprite;
    public string classType;
    

    public void getAdventData()
    {
        Debug.Log($"This Adventurer has id:{id} , name :{displayName} , classType:{classType}");
    }
}
