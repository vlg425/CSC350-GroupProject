using UnityEngine;
using System.Collections; // Required for Coroutines

public class PrefabSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject prefabToSpawn; // 1. Drag your Prefab here
    public GameObject targetObject; // 2. Drag the Camera/Target GameObject here
    public float spawnInterval = 2f;
    public float spawnRadius = 10f; // Distance from targetObject

    [Header("Despawn Settings")]
    public float despawnTime = 5f; // Time before the spawned object is destroyed

    private void Start()
    {
        // Start the continuous spawning routine
        StartCoroutine(SpawnRoutine());
    }

    private void SpawnPrefab()
    {
        // 1. Calculate the spawn position around the target
        Vector3 randomOffset = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPosition = targetObject.transform.position + new Vector3(randomOffset.x, randomOffset.y, 0);

        // 2. Instantiate the prefab at the calculated position
        GameObject newObject = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);

        // 3. Start the despawn timer for the newly spawned object
        StartCoroutine(DespawnAfterTime(newObject));
    }

    // Coroutine to handle continuous spawning
    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            // Wait for the specified interval
            yield return new WaitForSeconds(spawnInterval);
            // Then spawn the object
            SpawnPrefab();
        }
    }

    // Coroutine to destroy the object after a set time
    IEnumerator DespawnAfterTime(GameObject obj)
    {
        // Wait for the specified despawn time
        yield return new WaitForSeconds(despawnTime);

        // Check if the object still exists before destroying it
        if (obj != null)
        {
            Destroy(obj);
        }
    }
}