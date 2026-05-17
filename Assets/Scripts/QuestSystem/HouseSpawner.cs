using UnityEngine;

public class HouseSpawner : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints;

    public void SpawnObject(IngredientData ingredient, int amount)
    {

        if (ingredient == null || spawnPoints == null)
        {
            Debug.Log("HouseSpawner: ingredient/spawnPoints is null or empty");
        }

        if (ingredient == null || spawnPoints.Length == 0)
        {
            Debug.Log("HouseSpawner: spawnPoints not assigned");
        }

        for (int i = 0; i < amount; i++)
        {
            var v = spawnPoints[Random.Range(0, spawnPoints.Length)];
            Instantiate(ingredient.worldPrefab, v.position, v.rotation);


        }

    }
}
