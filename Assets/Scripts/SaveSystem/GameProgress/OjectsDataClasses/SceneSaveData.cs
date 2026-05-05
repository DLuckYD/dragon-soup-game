using System.Collections.Generic;

[System.Serializable]
public class SceneSaveData
{
    public List<SceneItemSaveData> items = new List<SceneItemSaveData>();
}

[System.Serializable]
public class SceneItemSaveData
{
    public string itemId;

    public float posX;
    public float posY;
    public float posZ;

    public float rotX;
    public float rotY;
    public float rotZ;
    public float rotW;

    public bool isActive;

    // for rewards
    public bool isUpgraded;
    public bool canBeUpgraded;
    public ItemType itemType;
    public int value;
}