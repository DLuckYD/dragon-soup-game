using UnityEngine;

public class SaveSystem
{
    public static SaveData saveData = new SaveData();

    [System.Serializable]
    public struct SaveData
    {
        public PlayerSaveData playerData;
    }

    public static string SaveFileName()
    {
        string saveFile = Application.persistentDataPath + "/savefile" + ".save";
        return saveFile;
    }
}
