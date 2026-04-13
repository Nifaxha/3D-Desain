using System.Collections;
using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    public GameObject[] foodPrefabs;
    public Transform[] spawnPoints;
    public float minSpawnTime = 1f;
    public float maxSpawnTime = 2.5f;

    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            float wait = Random.Range(minSpawnTime, maxSpawnTime);
            yield return new WaitForSeconds(wait);

            SpawnFood();
        }
    }

    void SpawnFood()
    {
        if (foodPrefabs.Length == 0 || spawnPoints.Length == 0) return;

        GameObject prefab = foodPrefabs[Random.Range(0, foodPrefabs.Length)];
        Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];

        Instantiate(prefab, point.position, Quaternion.identity);
    }
}