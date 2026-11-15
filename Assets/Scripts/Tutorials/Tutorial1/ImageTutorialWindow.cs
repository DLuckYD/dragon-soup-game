using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ImageTutorialWindow : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image tutorialImage;

    [SerializeField] private TMP_Text tutorialNumberText;
    [SerializeField] private TMP_Text tutorialNameText;
    [SerializeField] private TMP_Text tutorialDescriptionText;

    public void Initialize(ImageTutorialPage page)
    {
        if (tutorialImage != null)
        {
            tutorialImage.sprite = page.image;
            tutorialImage.enabled = page.image != null;
        }

        if (page.tutorialName != null)
        {
            tutorialNameText.text = page.tutorialName;
        }
        
        if (page.tutorialNumber != null)
        {
            tutorialNumberText.text = page.tutorialNumber.ToString();
        }
        
        if (page.tutorialDescription != null)
        {
            tutorialDescriptionText.text = page.tutorialDescription;
        }
        
        
        
        
    }
}