using UnityEngine;

public class SaveBlockerOnEnable : MonoBehaviour
{
    [SerializeField] private string blockerReason = "Quest offer window is open";

    private void OnEnable()
    {
        if (GameSaveLoadManager.Instance != null)
        {
            GameSaveLoadManager.Instance.AddSaveBlocker(blockerReason);
        }
    }

    private void OnDisable()
    {
        if (GameSaveLoadManager.Instance != null)
        {
            GameSaveLoadManager.Instance.RemoveSaveBlocker(blockerReason);
        }
    }
}
