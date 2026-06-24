using TMPro;
using UnityEngine;
using UnityEngine.UI;



public class DarkEntityUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button declineButton;

    [Header("Player Camera")]
    [SerializeField] private FirstPersonCamera cameraScript;

    private DarkEntity currentDarkEntity;
    private Recipe currentRecipe;
    
    

    private void Awake()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }

        if (acceptButton != null)
        {
            acceptButton.onClick.AddListener(OnAcceptClicked);
        }

        if (declineButton != null)
        {
            declineButton.onClick.AddListener(OnDeclineClicked);
        }
    }

    public void Open(DarkEntity darkEntity, Recipe recipe)
    {
        currentDarkEntity = darkEntity;
        currentRecipe = recipe;

        if (panel != null)
        {
            panel.SetActive(true);
        }

        FreezeGame();
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (descriptionText == null)
        {
            return;
        }

        if (currentRecipe == null)
        {
            descriptionText.text = "The Dark Entity cannot help you right now. There is no active recipe.";

            if (acceptButton != null)
            {
                acceptButton.interactable = false;
            }

            return;
        }

        descriptionText.text =
            $"The Dark Entity can restore the ingredients for:\n" +
            $"{currentRecipe.displayName}\n\n" +
            $"Accepting the deal will increase the dark influence.";

        if (acceptButton != null)
        {
            acceptButton.interactable = true;
        }
    }

    private void OnAcceptClicked()
    {
        if (currentDarkEntity != null)
        {
            currentDarkEntity.AcceptDeal();
        }

        Close();
    }

    private void OnDeclineClicked()
    {
        if (currentDarkEntity != null)
        {
            currentDarkEntity.DeclineDeal();
        }

        Close();
    }

    private void Close()
    {
        currentDarkEntity = null;
        currentRecipe = null;

        if (panel != null)
        {
            panel.SetActive(false);
        }

        UnfreezeGame();
    }

    private void FreezeGame()
    {
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (cameraScript != null)
        {
            cameraScript.canLook = false;
        }
    }

    private void UnfreezeGame()
    {
        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (cameraScript != null)
        {
            cameraScript.canLook = true;
        }
    }
}