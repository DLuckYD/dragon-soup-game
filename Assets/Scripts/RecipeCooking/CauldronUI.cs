using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CauldronUI : MonoBehaviour
{
    [SerializeField] private GameObject ingredientImagePrefab;
    [SerializeField] private Transform ingredientImagesContainer;
    [SerializeField] private GameObject timerContainer;
    [SerializeField] private TMP_Text timerText;

    [Header("Visual Settings")]
    [SerializeField] private float blinkSpeed = 3f;
    [SerializeField] private float inactiveAlpha = 0.35f;

    private readonly List<IngredientAmount> ingredients = new();
    private readonly List<CauldronIngredientIconUI> ingredientIcons = new();

    private int currentIngredientIndex = -1;
    private CauldronVisualState state = CauldronVisualState.None;

    public static event Action<string> OnWaitingIngredients;

    private enum CauldronVisualState
    {
        None,
        WaitingForIngredient,
        CookingIngredient,
        Finished
    }

    private void Awake()
    {
        ResetUI();
    }

    private void Update()
    {
        UpdateIconVisuals();
    }

    public void StartCookingUI(Recipe recipe)
    {
        if (recipe == null)
        {
            Debug.LogWarning("[CAULDRON UI] Cannot start UI. Recipe is null.");
            return;
        }

        ingredients.Clear();
        ingredients.AddRange(recipe.ingredients);

        GenerateIngredientImages();

        currentIngredientIndex = 0;
        state = CauldronVisualState.None;

        UpdateTimer(0f);

        Debug.Log("[CAULDRON UI] Started UI for recipe: " + recipe.displayName);
    }

    public void ShowWaitingForIngredient(int ingredientIndex, float time)
    {
        timerContainer.SetActive(true);
        currentIngredientIndex = ingredientIndex;
        state = CauldronVisualState.WaitingForIngredient;

        OnWaitingIngredients?.Invoke("Add next ingredient");

        timerText.color = Color.red;
        UpdateTimer(time);

        Debug.Log("[CAULDRON UI] Waiting for ingredient index: " + ingredientIndex);
    }

    public void ShowCookingIngredient(int ingredientIndex, float time)
    {
        currentIngredientIndex = ingredientIndex;
        state = CauldronVisualState.CookingIngredient;

        timerText.color = Color.black;
        UpdateTimer(time);

        if (ingredientIndex >= 0 && ingredientIndex < ingredientIcons.Count)
        {
            ingredientIcons[ingredientIndex].SetAlpha(1f);
        }

        Debug.Log("[CAULDRON UI] Cooking ingredient index: " + ingredientIndex);
    }

    public void MarkIngredientCooked(int ingredientIndex)
    {
        if (ingredientIndex >= 0 && ingredientIndex < ingredientIcons.Count)
        {
            ingredientIcons[ingredientIndex].SetAlpha(1f);
        }
    }

    public void UpdateTimer(float time)
    {
        if (timerText == null)
            return;

        int totalSeconds = Mathf.CeilToInt(time);

        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    public void ShowRecipeFinished()
    {
        state = CauldronVisualState.Finished;
        currentIngredientIndex = -1;

        UpdateTimer(0f);
        timerContainer.SetActive(false);

        foreach (CauldronIngredientIconUI icon in ingredientIcons)
        {
            if (icon != null)
                icon.SetAlpha(0f);
        }

        Debug.Log("[CAULDRON UI] Recipe finished.");
    }

    public void ResetUI()
    {
        state = CauldronVisualState.None;
        currentIngredientIndex = -1;

        UpdateTimer(0f);
        timerContainer.SetActive(false);

        if (ingredientImagesContainer != null)
        {
            foreach (Transform child in ingredientImagesContainer)
            {
                Destroy(child.gameObject);
            }
        }

        ingredients.Clear();
        ingredientIcons.Clear();
    }

    private void GenerateIngredientImages()
    {
        if (ingredientImagesContainer == null || ingredientImagePrefab == null)
        {
            Debug.LogWarning("[CAULDRON UI] Ingredient images container or prefab is not assigned.");
            return;
        }

        foreach (Transform child in ingredientImagesContainer)
        {
            Destroy(child.gameObject);
        }

        ingredientIcons.Clear();

        foreach (IngredientAmount ingredient in ingredients)
        {
            if (ingredient == null || ingredient.item == null)
                continue;

            GameObject obj = Instantiate(ingredientImagePrefab, ingredientImagesContainer);
            obj.SetActive(true);

            CauldronIngredientIconUI iconUI = obj.GetComponent<CauldronIngredientIconUI>();

            if (iconUI == null)
            {
                Debug.LogWarning("[CAULDRON UI] Ingredient image prefab has no CauldronIngredientIconUI.");
                continue;
            }

            iconUI.SetIngredient(ingredient);
            iconUI.SetAlpha(inactiveAlpha);

            ingredientIcons.Add(iconUI);
        }
    }

    private void UpdateIconVisuals()
    {
        if (state == CauldronVisualState.None || state == CauldronVisualState.Finished)
            return;

        for (int i = 0; i < ingredientIcons.Count; i++)
        {
            if (ingredientIcons[i] == null)
                continue;

            if (i < currentIngredientIndex)
            {
                ingredientIcons[i].SetAlpha(1f);
                continue;
            }

            if (i > currentIngredientIndex)
            {
                ingredientIcons[i].SetAlpha(inactiveAlpha);
                continue;
            }

            if (state == CauldronVisualState.WaitingForIngredient)
            {
                float blinkAlpha = Mathf.Lerp(
                    inactiveAlpha,
                    1f,
                    Mathf.PingPong(Time.time * blinkSpeed, 1f)
                );

                ingredientIcons[i].SetAlpha(blinkAlpha);
            }
            else if (state == CauldronVisualState.CookingIngredient)
            {
                ingredientIcons[i].SetAlpha(1f);
            }
        }
    }
}