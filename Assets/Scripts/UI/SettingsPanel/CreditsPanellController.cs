using UnityEngine;
using UnityEngine.UI;

public class CreditsPanellController : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button backToMenuButton;

    [SerializeField] private MainMenuUIManager mainMenuManager;

    private void Start()
    {
        backToMenuButton.onClick.AddListener(BackToMenu);
    }

    private void BackToMenu()
    {
        this.gameObject.SetActive(false);

        mainMenuManager.ShowStartScreen();
    }
}
