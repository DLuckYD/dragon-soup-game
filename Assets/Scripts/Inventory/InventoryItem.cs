using System.Collections.Generic;
using UnityEngine;

public abstract class InventoryItem : MonoBehaviour
{
    public IngredientData itemData;

    public bool isInInventory;
    public bool isHeld = false;

    protected Rigidbody rb;
    protected Transform objectGrabPointTransform;

    private Collider[] allColliders;
    private Collider[] physicalColliders;
    private Collider[] triggerColliders;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = GetComponentInChildren<Rigidbody>();

        allColliders = GetComponentsInChildren<Collider>(true);

        List<Collider> physical = new List<Collider>();
        List<Collider> triggers = new List<Collider>();

        foreach (Collider col in allColliders)
        {
            if (col.isTrigger)
                triggers.Add(col);
            else
                physical.Add(col);
        }

        physicalColliders = physical.ToArray();
        triggerColliders = triggers.ToArray();
    }

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

        SetHeldPhysicsState(true);


        if (name == "Bowl" || name == "Knife")
        {
            WwiseAudioManager.Instance.SetSwitchValue("Item_Type", "Metal", gameObject);
            WwiseAudioManager.Instance.PostEvent("Item_Pickup", gameObject);
        }

        //GetComponent<AxeDismantleRotation>()?.OnGrabbed();

        //Debug.Log($"[{name}] Grabbed");
    }

    public virtual void Drop()
    {
        objectGrabPointTransform = null;
        isHeld = false;

        //GetComponent<AxeDismantleRotation>()?.OnDropped();

        SetHeldPhysicsState(false);

        if (name == "Bowl" || name == "Knife")
        {
            WwiseAudioManager.Instance.SetSwitchValue("Item_Type", "Metal", gameObject);
            WwiseAudioManager.Instance.PostEvent("Item_Drop", gameObject);
        }

        //Debug.Log($"[{name}] Dropped");
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

    private void SetHeldPhysicsState(bool held)
    {
        if (rb != null)
        {
            rb.isKinematic = held;
            rb.useGravity = !held;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        foreach (Collider col in physicalColliders)
        {
            if (col != null)
                col.enabled = !held;
        }

        foreach (Collider col in triggerColliders)
        {
            if (col != null)
                col.enabled = !held;
        }
    }
}
