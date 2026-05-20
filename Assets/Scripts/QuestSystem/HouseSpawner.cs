using UnityEngine;
using System;



public class HouseSpawner : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints;
    
    public static HouseSpawner Instance { get; private set; }
    public static event Action<string> OnSpawned;
    

    public void SpawnObject(ItemData ingridient, int amount)
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
            var v = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
            Instantiate( ingridient.worldPrefab, v.position, v.rotation);
            OnSpawned?.Invoke("Items added in supply box ");
            


        }

    }
}
