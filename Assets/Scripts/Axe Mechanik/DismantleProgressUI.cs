using UnityEngine;
using UnityEngine.UI;

public class DismantleProgressUI : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private Slider progressSlider;

    private void Awake()
    {
        Hide();
    }

    public void Show()
    {
        if (root != null)
            root.SetActive(true);

        if (progressSlider != null)
        {
            progressSlider.gameObject.SetActive(true);
            progressSlider.value = 0f;
        }
    }

    public void SetProgress(float normalizedValue)
    {
        if (root != null && !root.activeSelf)
            root.SetActive(true);

        if (progressSlider != null)
        {
            if (!progressSlider.gameObject.activeSelf)
                progressSlider.gameObject.SetActive(true);

            progressSlider.value = Mathf.Clamp01(normalizedValue);
        }
    }

    public void Hide()
    {
        if (progressSlider != null)
            progressSlider.value = 0f;

        if (root != null)
            root.SetActive(false);
        else if (progressSlider != null)
            progressSlider.gameObject.SetActive(false);
    }
}