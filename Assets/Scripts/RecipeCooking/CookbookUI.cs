using UnityEngine;

public class CookbookUI : MonoBehaviour
{
    [Header("CookBook")]
    public GameObject cookBookPanel;
    public FirstPersonCamera cameraScript;

    [Header("Recipe UI")]
    [SerializeField] private RecipeDatabase recipeDatabase;
    [SerializeField] private Transform recipeListContainer;
    [SerializeField] private RecipeCardUI recipeCard;

    private bool isCookBookOpen = false;
    private bool recipesGenerated = false;

    private void Awake()
    {
        isCookBookOpen = false;

        if (cookBookPanel != null)
            cookBookPanel.SetActive(false);
    }

    public void OpenCookBook()
    {
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (cameraScript != null) cameraScript.canLook = false;

        isCookBookOpen = true;
        cookBookPanel.SetActive(true);

        if(!recipesGenerated)
        {
            GenerateRecipeCards();
            recipesGenerated = true;
        }
    }

    public bool IsOpened()
    {
        return isCookBookOpen;
    }

    public void CloseCookBook()
    {
        isCookBookOpen = false;
        cookBookPanel.SetActive(false);

        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        if (cameraScript != null) cameraScript.canLook = true;
    }

    void GenerateRecipeCards()
    {
        if (recipeDatabase == null || recipeCard == null || recipeListContainer == null)
        {
            Debug.LogWarning("CookbookUI: Missing references for generating recipe cards.");
            return;
        }

        int recipeCount = recipeDatabase.GetAllRecipes().Count;

        if (recipeCount == 0)
        {
            recipeCard.gameObject.SetActive(false);
            Debug.Log("No recipes found.");
            return;
        }

        // set the first recipe for the used template card
        recipeCard.gameObject.SetActive(true);
        recipeCard.SetRecipe(recipeDatabase.GetAllRecipes()[0]);

        // other recipes will be generated as new cards
        for (int i = 1; i < recipeCount; i++)
        {
            Recipe recipe = recipeDatabase.GetAllRecipes()[i];

            if (recipe == null)
                continue;

            RecipeCardUI newCard = Instantiate(recipeCard, recipeListContainer);
            newCard.gameObject.SetActive(true);
            newCard.SetRecipe(recipe);
        }
    }
}