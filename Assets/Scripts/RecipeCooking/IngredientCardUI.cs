using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngredientCardUI : MonoBehaviour
{
    [SerializeField] private TMP_Text imgredientName;
    [SerializeField] private Image image;

    public void SetIngredient(IngredientAmount ingredient)
    {
        if (imgredientName != null)
            imgredientName.text = $"x{ingredient.amount} - {ingredient.item.displayName}";
        if (image != null)
            image.sprite = ingredient.item.icon;
    }
}
