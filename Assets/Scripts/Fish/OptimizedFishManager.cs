using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class OptimizedFishManager : MonoBehaviour
{
    [Header("Fish Settings")]
    public GameObject fishPrefab;
    public int spawnRatePerSecond = 500;
    public Vector3 spawnAreaSize = new Vector3(100, 50, 100);
    public int minTotalFish = 20000;
    public int maxTotalFish = 60000;
    public float maxSpeed = 10f;

    [Header("Performance Settings")]
    public int maxFishPerFrame = 100; // 每帧最多处理的鱼数量
    public float spatialGridSize = 10f; // 空间网格大小

    [Header("Debug UI")]
    public TextMeshProUGUI fishCountText;

    public class FishData
    {
        public Transform transform;
        public Vector3 velocity;
        public Vector3 position;
        public int poolIndex;
        public bool isActive;
        public float speed;
        public Color fishColor;
    }

    // 对象池
    public Queue<GameObject> fishPool = new Queue<GameObject>();
    public List<FishData> activeFish = new List<FishData>();
    public List<FishData> inactiveFish = new List<FishData>();

    // 空间分割网格
    private Dictionary<Vector3Int, List<FishData>> spatialGrid = new Dictionary<Vector3Int, List<FishData>>();

    // 性能优化变量
    private int currentProcessIndex = 0;
    private float lastUIUpdate = 0f;
    private const float UI_UPDATE_INTERVAL = 0.1f;

    // 鱼的种类颜色
    private Color[] fishColors = { Color.red, Color.blue, Color.green, Color.yellow, Color.cyan };

    void Start()
    {
        InitializePool();
        StartCoroutine(FishSpawnLoop());
    }

    void InitializePool()
    {
        // 预创建对象池
        for (int i = 0; i < 1000; i++)
        {
            GameObject fish = Instantiate(fishPrefab);
            fish.SetActive(false);
            fish.transform.SetParent(transform);
            fishPool.Enqueue(fish);
        }
    }

    void Update()
    {
        UpdateFishMovementBatch();
        UpdateSpatialGrid();
        UpdateUI();
    }

    void UpdateFishMovementBatch()
    {
        if (activeFish.Count == 0) return;

        // 批量处理，每帧只处理部分鱼
        int fishToProcess = Mathf.Min(maxFishPerFrame, activeFish.Count);
        int endIndex = Mathf.Min(currentProcessIndex + fishToProcess, activeFish.Count);

        for (int i = currentProcessIndex; i < endIndex; i++)
        {
            var fish = activeFish[i];
            if (!fish.isActive) continue;

            // 简化的移动逻辑
            fish.position += fish.velocity * Time.deltaTime;

            // 边界检查和重置
            if (!IsInsideBounds(fish.position))
            {
                fish.position = GetRandomPosition();
                fish.velocity = GetRandomVelocity();
            }

            fish.transform.position = fish.position;

            // 简单的旋转
            if (fish.velocity.sqrMagnitude > 0.1f)
            {
                fish.transform.rotation = Quaternion.LookRotation(fish.velocity);
            }
        }

        // 更新处理索引
        currentProcessIndex = endIndex;
        if (currentProcessIndex >= activeFish.Count)
        {
            currentProcessIndex = 0;
        }
    }

    void UpdateSpatialGrid()
    {
        // 清空网格（简化版，实际项目中可以用更高效的方式）
        spatialGrid.Clear();

        // 将鱼分配到网格中
        foreach (var fish in activeFish)
        {
            if (!fish.isActive) continue;

            Vector3Int gridPos = GetGridPosition(fish.position);
            if (!spatialGrid.ContainsKey(gridPos))
            {
                spatialGrid[gridPos] = new List<FishData>();
            }
            spatialGrid[gridPos].Add(fish);
        }
    }

    Vector3Int GetGridPosition(Vector3 position)
    {
        return new Vector3Int(
            Mathf.FloorToInt(position.x / spatialGridSize),
            Mathf.FloorToInt(position.y / spatialGridSize),
            Mathf.FloorToInt(position.z / spatialGridSize)
        );
    }

    void UpdateUI()
    {
        if (Time.time - lastUIUpdate > UI_UPDATE_INTERVAL && fishCountText != null)
        {
            fishCountText.text = $"Fish Count: {activeFish.Count}";
            lastUIUpdate = Time.time;
        }
    }

    bool IsInsideBounds(Vector3 pos)
    {
        Vector3 center = transform.position;
        Vector3 halfSize = spawnAreaSize / 2f;
        return Mathf.Abs(pos.x - center.x) < halfSize.x &&
               Mathf.Abs(pos.y - center.y) < halfSize.y &&
               Mathf.Abs(pos.z - center.z) < halfSize.z;
    }

    IEnumerator FishSpawnLoop()
    {
        while (true)
        {
            // 生成新鱼
            int toSpawn = UnityEngine.Random.Range(100, spawnRatePerSecond + 1);
            for (int i = 0; i < toSpawn && activeFish.Count < maxTotalFish; i++)
            {
                SpawnFish();
            }

            // 移除过多的鱼
            int overage = activeFish.Count - UnityEngine.Random.Range(minTotalFish, maxTotalFish);
            for (int i = 0; i < Mathf.Max(0, overage); i++)
            {
                RemoveRandomFish();
            }

            yield return new WaitForSeconds(1f);
        }
    }

    void SpawnFish()
    {
        GameObject fishObj;

        // 从对象池获取或创建新对象
        if (fishPool.Count > 0)
        {
            fishObj = fishPool.Dequeue();
        }
        else
        {
            fishObj = Instantiate(fishPrefab);
            fishObj.transform.SetParent(transform);
        }

        Vector3 position = GetRandomPosition();
        fishObj.transform.position = position;
        fishObj.SetActive(true);

        // 设置鱼的颜色（可选功能4）
        Color fishColor = fishColors[Random.Range(0, fishColors.Length)];
        var renderer = fishObj.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = fishColor;
        }

        var fishData = new FishData
        {
            transform = fishObj.transform,
            velocity = GetRandomVelocity(),
            position = position,
            isActive = true,
            speed = Random.Range(1f, maxSpeed),
            fishColor = fishColor
        };

        activeFish.Add(fishData);
    }

    void RemoveRandomFish()
    {
        if (activeFish.Count == 0) return;

        int index = Random.Range(0, activeFish.Count);
        var fish = activeFish[index];

        fish.transform.gameObject.SetActive(false);
        fishPool.Enqueue(fish.transform.gameObject);

        activeFish.RemoveAt(index);
    }

    Vector3 GetRandomPosition()
    {
        return transform.position + new Vector3(
            Random.Range(-spawnAreaSize.x / 2f, spawnAreaSize.x / 2f),
            Random.Range(-spawnAreaSize.y / 2f, spawnAreaSize.y / 2f),
            Random.Range(-spawnAreaSize.z / 2f, spawnAreaSize.z / 2f)
        );
    }

    Vector3 GetRandomVelocity()
    {
        return Random.onUnitSphere * Random.Range(1f, maxSpeed);
    }

    // 可选功能：获取附近的鱼（用于碰撞检测等）
    public List<FishData> GetNearbyFish(Vector3 position, float radius)
    {
        List<FishData> nearbyFish = new List<FishData>();
        Vector3Int gridPos = GetGridPosition(position);

        // 检查周围的网格
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    Vector3Int checkPos = gridPos + new Vector3Int(x, y, z);
                    if (spatialGrid.ContainsKey(checkPos))
                    {
                        foreach (var fish in spatialGrid[checkPos])
                        {
                            if (Vector3.Distance(position, fish.position) <= radius)
                            {
                                nearbyFish.Add(fish);
                            }
                        }
                    }
                }
            }
        }

        return nearbyFish;
    }

    // 可选功能：移除特定位置的鱼（用于碰撞）
    public void RemoveFishAt(Vector3 position, float radius)
    {
        var nearbyFish = GetNearbyFish(position, radius);
        foreach (var fish in nearbyFish)
        {
            fish.transform.gameObject.SetActive(false);
            fishPool.Enqueue(fish.transform.gameObject);
            activeFish.Remove(fish);
        }
    }
}
