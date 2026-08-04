[System.Serializable]
public class GameSaveData
{
    public int version = 1;

    public string saveName;
    public string sceneName;
    public string savedAt;

    public InventorySaveData inventory = new InventorySaveData();
    public SceneSaveData sceneObjects = new SceneSaveData();
    public PlayerSaveData player = new PlayerSaveData();
    public RecipeSaveData recipe = new RecipeSaveData();
    public QuestSaveData quests = new QuestSaveData();

}