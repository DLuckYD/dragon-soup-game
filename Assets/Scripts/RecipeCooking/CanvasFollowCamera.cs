using UnityEngine;

public class CanvasFollowCamera : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Transform cameraTransform;

    [Header("Billboard Settings")]
    [SerializeField] private bool rotateOnY = true;

    private void Awake()
    {
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    private void LateUpdate()
    {
        if (cameraTransform == null)
            return;

        Vector3 direction = transform.position - cameraTransform.position;

        if (rotateOnY)
            direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        transform.rotation = Quaternion.LookRotation(direction);
    }
}