using UnityEngine;

[System.Serializable]
public class PlayerSaveData
{
    public float posX;
    public float posY;
    public float posZ;

    public float orientationYaw;

    public int activeHotbarIndex = -1;
}