using UnityEngine;
using UnityEngine.UI;

public class CauldronIngredientIconUI : MonoBehaviour
{
    [SerializeField] private Image icon;

    public void SetIngredient(IngredientAmount ingredient)
    {
        if (icon != null && ingredient != null && ingredient.item != null)
        {
            icon.sprite = ingredient.item.icon;
            icon.preserveAspect = true;
        }
    }

    public void SetAlpha(float alpha)
    {
        if (icon != null)
            icon.color = new Color(icon.color.r, icon.color.g, icon.color.b, alpha);
    }
}
