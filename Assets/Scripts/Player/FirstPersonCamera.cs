using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{

    public Transform player;
    public float cameraVerticalRotation = 0f;
    public float cameraHorizontalRotation = 0f;
    public bool lockCursor = true;
    public bool canLook = true;

    private float mouseSensitivity;

    void Start()
    {
        if (player == null)
        {
            if (transform.parent != null)
                player = transform.parent;
            else
                Debug.LogWarning("FirstPersonCamera: player Transform not assigned and camera has no parent. Assign a player Transform in the Inspector.");
        }

        cameraHorizontalRotation = player != null ? player.localEulerAngles.y : 0f;

        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (SettingsSaveLoadManager.Instance != null)
        {
            mouseSensitivity = SettingsSaveLoadManager.Instance.CurrentSettings.mouseSensitivity;
        }
        else
        {
            mouseSensitivity = 1f;
            Debug.LogWarning("FirstPersonCamera: SettingsSaveLoadManager instance not found. Using default mouse sensitivity.");
        }
    }

    void Update()
    {
        if (!canLook)
        {
            return;
        }
        
        // collecting mouse input
        float inputX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float inputY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // resresentation of camera vertical rotation
        cameraVerticalRotation -= inputY;
        cameraVerticalRotation = Mathf.Clamp(cameraVerticalRotation, -90f, 90);
        transform.localEulerAngles = Vector3.right * cameraVerticalRotation;

        // resresentation of camera horizontal rotation
        player.Rotate(Vector3.up * inputX);
    }

}
