// =====================================================
// ItemSpawner.cs - Handles randomized item spawning for pirate ship
// =====================================================

using UnityEngine;
using System.Collections.Generic;

public class ItemSpawner : MonoBehaviour
{
    [Header("Spawn Configuration")]
    public List<Transform> spawnPoints = new List<Transform>();
    public List<PirateItemData> itemsToSpawn = new List<PirateItemData>();
    public bool shuffleSpawnOrder = true;

    [Header("Spawn Behavior")]
    public bool spawnOnStart = true;
    public bool preventDuplicateLocations = true;

    [Header("Debug")]
    public bool showSpawnGizmos = true;
    public Color spawnGizmoColor = Color.green;

    private List<Transform> usedSpawnPoints = new List<Transform>();

    void Start()
    {
        if (spawnOnStart)
        {
            SpawnAllItems();
        }
    }

    public void SpawnAllItems()
    {
        if (spawnPoints.Count == 0)
        {
            Debug.LogError("No spawn points assigned to ItemSpawner!");
            return;
        }

        usedSpawnPoints.Clear();
        List<Transform> availablePoints = new List<Transform>(spawnPoints);

        if (shuffleSpawnOrder)
        {
            ShuffleList(availablePoints);
        }

        foreach (var itemData in itemsToSpawn)
        {
            SpawnItem(itemData, availablePoints);
        }
    }

    void SpawnItem(PirateItemData itemData, List<Transform> availablePoints)
    {
        for (int i = 0; i < itemData.requiredAmount; i++)
        {
            Transform spawnPoint = GetNextSpawnPoint(availablePoints);
            if (spawnPoint == null)
            {
                Debug.LogWarning($"No available spawn points for {itemData.displayName}");
                break;
            }

            GameObject spawnedItem = Instantiate(itemData.prefab, spawnPoint.position, spawnPoint.rotation);

            // Setup collectible component
            PirateCollectibleItem collectible = spawnedItem.GetComponent<PirateCollectibleItem>();
            if (collectible == null)
            {
                collectible = spawnedItem.AddComponent<PirateCollectibleItem>();
            }
            collectible.SetItemData(itemData);

            // Mark spawn point as used if preventing duplicates
            if (preventDuplicateLocations)
            {
                usedSpawnPoints.Add(spawnPoint);
                availablePoints.Remove(spawnPoint);
            }

            Debug.Log($"Spawned {itemData.displayName} at {spawnPoint.name}");
        }
    }

    Transform GetNextSpawnPoint(List<Transform> availablePoints)
    {
        if (availablePoints.Count == 0) return null;

        Transform selectedPoint = availablePoints[0];
        return selectedPoint;
    }

    void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    public void ClearAllSpawnedItems()
    {
        PirateCollectibleItem[] spawnedItems = FindObjectsOfType<PirateCollectibleItem>();
        foreach (var item in spawnedItems)
        {
            if (Application.isPlaying)
            {
                Destroy(item.gameObject);
            }
            else
            {
                DestroyImmediate(item.gameObject);
            }
        }
        usedSpawnPoints.Clear();
    }

    public void RespawnAllItems()
    {
        ClearAllSpawnedItems();
        SpawnAllItems();
    }

    void OnDrawGizmos()
    {
        if (!showSpawnGizmos) return;

        Gizmos.color = spawnGizmoColor;
        foreach (var spawnPoint in spawnPoints)
        {
            if (spawnPoint != null)
            {
                Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
                Gizmos.DrawLine(spawnPoint.position, spawnPoint.position + Vector3.up * 2f);
            }
        }

        // Show used spawn points differently in play mode
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            foreach (var usedPoint in usedSpawnPoints)
            {
                if (usedPoint != null)
                {
                    Gizmos.DrawSphere(usedPoint.position, 0.3f);
                }
            }
        }
    }
}