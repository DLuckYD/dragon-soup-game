using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HotbarSlot : MonoBehaviour
{
    public Image background;
    public Image icon;
    public Image highlitedBack;
    public TMP_Text amountText;

    [Header("Stackable Data")]
    public IngredientData itemData;
    public int amount = 0;
    public Color originalColor;

    [Header("Non-stackable Data")]
    public InventoryItem uniqueItem;

    public bool IsEmpty =>
       (itemData == null || amount <= 0) && uniqueItem == null;

    private void Awake()
    {
        if (background != null)
            originalColor = background.color;
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
            }

            if (amountText != null)
                amountText.text = "";

            return;
        }

        if (icon != null)
        {
            icon.enabled = true;
            icon.sprite = data.icon;
        }

        if (amountText != null)
        {
            amountText.text = (uniqueItem == null && data.isStackable && amount > 1)
                ? amount.ToString()
                : "";
        }
    }
}
