using System.Collections;
using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    [Header("Food Settings")]
    public GameObject[] foodPrefabs;
    public Transform[] spawnPoints;
    public float minSpawnTime = 1f;
    public float maxSpawnTime = 2.5f;

    [Header("Mystery Box Settings")]
    public GameObject mysteryBoxPrefab; // Masukkan prefab Mystery Box di sini
    public float mysteryBoxCooldown = 10f; // Atur waktu jeda (cooldown) Mystery Box di sini

    void Start()
    {
        // Menjalankan loop spawn makanan dan mystery box secara bersamaan
        StartCoroutine(SpawnFoodLoop());
        StartCoroutine(SpawnMysteryBoxLoop());
    }

    IEnumerator SpawnFoodLoop()
    {
        while (true)
        {
            float wait = Random.Range(minSpawnTime, maxSpawnTime);
            yield return new WaitForSeconds(wait);

            if (foodPrefabs.Length > 0)
            {
                GameObject prefab = foodPrefabs[Random.Range(0, foodPrefabs.Length)];
                SpawnObject(prefab);
            }
        }
    }

    IEnumerator SpawnMysteryBoxLoop()
    {
        while (true)
        {
            // Menunggu selama waktu cooldown khusus Mystery Box
            yield return new WaitForSeconds(mysteryBoxCooldown);

            if (mysteryBoxPrefab != null)
            {
                SpawnObject(mysteryBoxPrefab);
            }
        }
    }

    // Fungsi ini digunakan oleh makanan maupun Mystery Box untuk muncul di titik acak yang sama
    void SpawnObject(GameObject prefabToSpawn)
    {
        if (spawnPoints.Length == 0) return;

        // Memilih titik dari array spawnPoints (titik yang sama dengan makanan)
        Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];

        Instantiate(prefabToSpawn, point.position, Quaternion.identity);
    }
}