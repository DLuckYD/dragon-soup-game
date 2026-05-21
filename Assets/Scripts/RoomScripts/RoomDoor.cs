using System;
using UnityEngine;

public class RoomDoor : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private string doorId;
    [SerializeField] private bool isLocked = true;
    [SerializeField] private EffectType effectType = EffectType.UnlockRoom;

    [Header("Opening Settings")]
    [SerializeField] private Transform hingePoint;
    [SerializeField] private float openAngle = -90f;
    [SerializeField] private Collider interactionDoorCollider;

    private Vector3 closedPosition;
    private Quaternion closedRotation;
    public string GetDoorId => doorId;
    public bool IsLocked => isLocked;

    private void Awake()
    {
        closedPosition = transform.position;
        closedRotation = transform.rotation;
    }
    internal void UnlockDoor()
    {
        if (!isLocked)
        {
            Debug.Log($"Door {doorId} is already unlocked.");
            return;
        }
        else
        {
            OpenDoor();
            isLocked = false;
        }
    }

    private void OpenDoor()
    {
        if (hingePoint == null)
        {
            Debug.LogWarning($"Door {doorId} has no hinge point assigned.");
            return;
        }

        Quaternion rotation = Quaternion.AngleAxis(openAngle, Vector3.up);

        Vector3 directionFromHinge = closedPosition - hingePoint.position;

        transform.position = hingePoint.position + rotation * directionFromHinge;
        transform.rotation = rotation * closedRotation;

        if (interactionDoorCollider != null)
        {
            interactionDoorCollider.enabled = false;
        }
    }
}