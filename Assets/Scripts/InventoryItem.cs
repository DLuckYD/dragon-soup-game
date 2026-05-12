using UnityEngine;

public abstract class InventoryItem : MonoBehaviour
{
    public ItemData itemData;

    public bool isInInventory;
    public bool isHeld = false;

    [Header("Save Data")]
    [SerializeField] private string saveId;
    public string SaveId => saveId;
    public bool HasSaveId => !string.IsNullOrEmpty(saveId);

    protected Rigidbody rb;
    protected Transform objectGrabPointTransform;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = GetComponentInChildren<Rigidbody>();

    }

    public void EnsureRuntimeSaveId()
    {
        if (string.IsNullOrEmpty(saveId))
        {
            saveId = System.Guid.NewGuid().ToString();
        }
    }

    public void GenerateNewSaveId()
    {
        saveId = System.Guid.NewGuid().ToString();

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

#if UNITY_EDITOR
    [ContextMenu("Generate Save ID")]
    private void GenerateSaveIdInEditor()
    {
        GenerateNewSaveId();
    }
#endif

    protected virtual void LateUpdate()
    {
        if (objectGrabPointTransform != null)
        {
            transform.position = objectGrabPointTransform.position;
            transform.rotation = objectGrabPointTransform.rotation;
        }
    }

    public virtual void Grab(Transform grabPoint)
    {
        objectGrabPointTransform = grabPoint;
        isHeld = true;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
        }
        if(name == "Bowl" || name == "Knife")
        {
            AkUnitySoundEngine.SetSwitch("Item_Type", "Metal", gameObject);
            AkUnitySoundEngine.PostEvent("Item_Pickup", gameObject);
        }
        Debug.Log($"[{name}] Grabbed");
    }

    public virtual void Drop()
    {
        objectGrabPointTransform = null;
        isHeld = false;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;
        }

        if (name == "Bowl" || name == "Knife")
        {
            AkUnitySoundEngine.SetSwitch("Item_Type", "Metal", gameObject);
            AkUnitySoundEngine.PostEvent("Item_Drop", gameObject);
        }

        Debug.Log($"[{name}] Dropped");
    }

    public bool isStackable()
    {
        return itemData != null && itemData.isStackable;
    }

    public int getMaxStackSize()
    {
        return itemData != null ? itemData.maxStackSize : 1;
    }

    public Sprite GetIcon()
    {
        return itemData != null ? itemData.icon : null;
    }
}
