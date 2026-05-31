using UnityEngine;

public class Cookbook : MonoBehaviour
{
    [Header("CookBook")]
    public GameObject cookBookPanel;
    public FirstPersonCamera cameraScript;

    private bool isCookBookOpen = false;

    private void Awake()
    {
        isCookBookOpen = false;

        if (cookBookPanel != null)
            cookBookPanel.SetActive(false);
    }

    public void OpenCookBook()
    {
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        if (cameraScript != null) cameraScript.canLook = false;

        isCookBookOpen = true;
        cookBookPanel.SetActive(true);
    }

    public bool IsOpened()
    {
        return isCookBookOpen;
    }

    public void CloseCookBook()
    {
        isCookBookOpen = false;
        cookBookPanel.SetActive(false);

        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        if (cameraScript != null) cameraScript.canLook = true;
    }
}