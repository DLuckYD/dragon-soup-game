using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ImageTutorialWindow : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image tutorialImage;
    [SerializeField] private TMP_Text tutorialText;

    public void Initialize(Sprite image, string text)
    {
        if (tutorialImage != null)
        {
            tutorialImage.sprite = image;
            tutorialImage.enabled = image != null;
        }

        if (tutorialText != null)
        {
            tutorialText.text = text;
        }
    }
}