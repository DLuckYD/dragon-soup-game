using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HotbarSlot : MonoBehaviour
{
    [Header("UI")]
    public Image background;
    public Image iconBackground;
    public Image icon;
    public TMP_Text amountText;

    [Header("Colors")]
    public Color normalBackgroundColor = Color.white;
    public Color highlightedBackgroundColor = Color.yellow;

    [Header("Stackable Data")]
    public IngredientData itemData;
    public int amount = 0;

    [Header("Non-stackable Data")]
    public InventoryItem uniqueItem;

    public bool IsEmpty =>
        (itemData == null || amount <= 0) && uniqueItem == null;

    private void Awake()
    {
        if (background != null)
            normalBackgroundColor = background.color;
    }

    public void Clear()
    {
        itemData = null;
        amount = 0;
        uniqueItem = null;
        UpdateUI();
    }

    public void UpdateUI()
    {
        IngredientData data = uniqueItem != null ? uniqueItem.itemData : itemData;

        if (data == null || (uniqueItem == null && amount <= 0))
        {
            if (icon != null)
            {
                icon.enabled = false;
                icon.sprite = null;
                icon.color = Color.white;
            }

            if (iconBackground != null)
                iconBackground.enabled = false;

            if (amountText != null)
                amountText.text = "";

            return;
        }

        if (iconBackground != null)
        {
            iconBackground.enabled = true;
            iconBackground.color = Color.white;
        }

        if (icon != null)
        {
            icon.enabled = true;
            icon.sprite = data.icon;

            icon.color = Color.white;
        }

        if (amountText != null)
        {
            amountText.text = (uniqueItem == null && data.isStackable && amount > 1)
                ? amount.ToString()
                : "";
        }
    }

    public void SetHighlighted(bool highlighted)
    {
        //Debug.Log(gameObject.name + " highlighted: " + highlighted);

        if (background != null)
            background.color = highlighted ? highlightedBackgroundColor : normalBackgroundColor;
        else
            Debug.LogWarning(gameObject.name + " background is NULL");
    }
}