using UnityEngine;
using UnityEngine.UI;

[System.Serializable]


public class AchievementsPanelController : MonoBehaviour
{
        [Header("Buttons")]
        
        [SerializeField] private Button backToMenuButton;
        [SerializeField] private MainMenuUIManager mainMenuManager;

        private void Start()
        {
            backToMenuButton.onClick.AddListener(BackToMenu);

        }




        public void BackToMenu()
        {
            WwiseAudioManager.Instance.PostEvent("Ui_Button_Cancel", this.gameObject);
            this.gameObject.SetActive(false);
            mainMenuManager.ShowStartScreen();
        }
    }
