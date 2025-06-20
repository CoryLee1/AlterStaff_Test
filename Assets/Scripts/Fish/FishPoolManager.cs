using System.Collections.Generic;
using UnityEngine;

public class FishPoolManager : MonoBehaviour
{
    [Header("Fish Settings")]
    public GameObject fishPrefab;
    public int initialPoolSize = 1000;
    public int maxPoolSize = 60000;

    private Queue<GameObject> fishPool = new Queue<GameObject>();

    public static FishPoolManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject fish = Instantiate(fishPrefab);
            fish.SetActive(false);
            fishPool.Enqueue(fish);
        }
    }

    public GameObject SpawnFish(Vector3 position, Quaternion rotation)
    {
        GameObject fish;
        if (fishPool.Count > 0)
        {
            fish = fishPool.Dequeue();
        }
        else if (TotalFishCount() < maxPoolSize)
        {
            fish = Instantiate(fishPrefab);
        }
        else
        {
            return null; // 超出最大限制
        }

        fish.transform.position = position;
        fish.transform.rotation = rotation;
        fish.SetActive(true);
        return fish;
    }

    public void ReturnFish(GameObject fish)
    {
        fish.SetActive(false);
        fishPool.Enqueue(fish);
    }

    public int TotalFishCount()
    {
        return fishPool.Count + GameObject.FindGameObjectsWithTag("Fish").Length;
    }
}
