using UnityEngine;
using System.Collections; 

public class SpawnManager : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject prefabToSpawn;
    public GameObject targetObject;
    public float spawnInterval = 2f;
    public float spawnRadius = 10f;

    [Header("Collision Avoidance")]
    public string exclusionTag = "Island";
    public float minDistanceBuffer = 0.5f; 
    public int maxSpawnAttempts = 10; 

    [Header("Despawn Settings")]
    public float despawnTime = 5f;

    private Collider2D[] IslandColliders;
    private Collider2D NodeCollider;

    private void Start()
    {
        // 1. Find all islands when the scene starts.
        FindAllIslands();
        
        // 2. Store the node's collider
        NodeCollider = prefabToSpawn.GetComponent<Collider2D>();
        
        // 3. Start the continuous spawning
        StartCoroutine(Spawn());
    }

    void FindAllIslands()
    {
        GameObject[] islandObjects = GameObject.FindGameObjectsWithTag(exclusionTag);
        IslandColliders = new Collider2D[islandObjects.Length];

        for (int i = 0; i < islandObjects.Length; i++)
        {
            if (islandObjects[i] != null)
            {
                IslandColliders[i] = islandObjects[i].GetComponent<Collider2D>();
            }
        }
    }
    

    private void SpawnPrefab()
    {
        Vector3 spawnPosition = Vector3.zero;
        int attempts = 0;

        while (attempts < maxSpawnAttempts)
        {
            attempts++;

            Vector3 randomOffset = Random.insideUnitCircle * spawnRadius;
            Vector3 candidatePosition = targetObject.transform.position + new Vector3(randomOffset.x, randomOffset.y, 0);

            if (IsPositionSafe(candidatePosition))
            {
                spawnPosition = candidatePosition;
                GameObject newObject = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
                StartCoroutine(DespawnAfterTime(newObject));
                return;
            }
        }
        
        Debug.LogWarning($"Failed to find a safe spawn position for {prefabToSpawn.name} after {maxSpawnAttempts} attempts.");
    }
    
    private bool IsPositionSafe(Vector3 position)
    {
        if (NodeCollider == null) return true;
        
        foreach (Collider2D islandCollider in IslandColliders)
        {
            if (islandCollider != null)
            {
                if (islandCollider.OverlapPoint(position))
                {
                    return false;
                }

                float requiredSeparation = islandCollider.bounds.extents.x + NodeCollider.bounds.extents.x + minDistanceBuffer;
                float actualDistance = Vector2.Distance(position, islandCollider.transform.position);

                if (actualDistance < requiredSeparation)
                {
                    return false;
                }
            }
        }
        return true;
    }

    IEnumerator Spawn()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnPrefab();
        }
    }

    IEnumerator DespawnAfterTime(GameObject obj)
    {
        yield return new WaitForSeconds(despawnTime);

        if (obj != null)
        {
            Destroy(obj);
        }
    }
}