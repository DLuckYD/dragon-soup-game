using UnityEngine;

public class PlayerSaveManager : MonoBehaviour
{
    public static PlayerSaveManager Instance { get; private set; }

    [Header("Player References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform orientationTransform;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerInteraction playerInteraction;


    private void Awake()
    {
        Instance = this;


        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();

        if (playerTransform == null)
        {
            playerTransform = playerMovement.transform;
        }

        if (playerInteraction == null)
            playerInteraction = GetComponent<PlayerInteraction>();

        if (orientationTransform == null && playerMovement != null)
            orientationTransform = playerMovement.orientation;

    }

    public PlayerSaveData CaptureSaveData()
    {
        PlayerSaveData saveData = new PlayerSaveData();

        if (playerTransform == null)
        {
            Debug.LogWarning("[PLAYER SAVE] playerTransform is NULL.");
            return saveData;
        }


        Vector3 position = playerTransform.position;
        Quaternion playerRotation = playerTransform.rotation;

        saveData.posX = position.x;
        saveData.posY = position.y;
        saveData.posZ = position.z;

        if (playerMovement != null && playerMovement.orientation != null)
        {
            saveData.orientationYaw = playerMovement.orientation.eulerAngles.y;
        }
        else
        {
            Debug.LogWarning("[PLAYER SAVE] PlayerMovement or orientation is NULL.");
        }

        if (playerInteraction == null)
        {
            return saveData;
        }
        
        saveData.activeHotbarIndex = playerInteraction.GetActiveHotbarIndex();

        return saveData;
    }

    public void RestoreSaveData(PlayerSaveData data)
    {

        if (data == null)
        {
            Debug.LogWarning("[PLAYER LOAD] Save data is NULL.");
            return;
        }

        if (playerTransform == null)
        {
            Debug.LogWarning("[PLAYER LOAD] playerTransform is NULL.");
            return;
        }

        Vector3 position = new Vector3(data.posX, data.posY, data.posZ);

        Quaternion yawRotation = Quaternion.Euler(0f, data.orientationYaw, 0f);

        Rigidbody rb = playerTransform.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.position = position;
        }
        else
        {
            playerTransform.position = position;
        }

        // the direction of the move = orientation
        if (playerMovement != null && playerMovement.orientation != null)
        {
            playerMovement.orientation.rotation = yawRotation;
        }

        // player body should look in the same direction as the orientation
        playerTransform.rotation = yawRotation;

        Physics.SyncTransforms();

        if (playerInteraction != null)
        {
            playerInteraction.RestoreActiveHotbarSlot(data.activeHotbarIndex);
        }
    }
}
