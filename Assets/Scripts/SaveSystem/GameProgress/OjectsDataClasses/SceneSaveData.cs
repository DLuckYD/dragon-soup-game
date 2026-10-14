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

    // Scale
    public float scaleX = 1f;
    public float scaleY = 1f;
    public float scaleZ = 1f;

    // Physics
    public bool hasRigidbody;
    public bool isKinematic;
    public bool useGravity;
    public bool detectCollisions;
    public int rigidbodyConstraints;

    public bool isActive;

    // for rewards
    public bool isUpgraded;
    public bool canBeUpgraded;
    public ItemType itemType;
    public int value;
}