using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FishManager : MonoBehaviour
{
    [Header("Fish Settings")]
    public GameObject fishPrefab;
    public int spawnRatePerSecond = 500;
    public Vector3 spawnAreaSize = new Vector3(100, 50, 100);
    public int minTotalFish = 20000;
    public int maxTotalFish = 60000;
    public float maxSpeed = 10f;

    [Header("Debug UI")]
    public TextMeshProUGUI fishCountText;

    private class FishData
    {
        public GameObject fishObject;
        public Vector3 velocity;
    }

    private List<FishData> fishList = new List<FishData>();

    void Start()
    {
        StartCoroutine(FishSpawnLoop());
    }

    void Update()
    {
        UpdateFishMovement();

        if (fishCountText != null)
            fishCountText.text = "Fish Count: " + fishList.Count;
    }

    void UpdateFishMovement()
    {
        foreach (var fish in fishList)
        {
            Vector3 pos = fish.fishObject.transform.position;
            pos += fish.velocity * Time.deltaTime;

            // Reset if out of bounds
            if (!IsInsideBounds(pos))
            {
                pos = GetRandomPosition();
            }

            fish.fishObject.transform.position = pos;
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
            int toSpawn = UnityEngine.Random.Range(100, spawnRatePerSecond + 1);

            for (int i = 0; i < toSpawn && fishList.Count < maxTotalFish; i++)
            {
                Vector3 pos = GetRandomPosition();
                GameObject fishObj = Instantiate(fishPrefab, pos, Quaternion.identity);
                fishObj.transform.SetParent(transform);

                Vector3 velocity = UnityEngine.Random.onUnitSphere * UnityEngine.Random.Range(1f, maxSpeed);
                fishList.Add(new FishData { fishObject = fishObj, velocity = velocity });
            }

            int overage = fishList.Count - UnityEngine.Random.Range(minTotalFish, maxTotalFish);
            for (int i = 0; i < Mathf.Max(0, overage); i++)
            {
                int index = UnityEngine.Random.Range(0, fishList.Count);
                Destroy(fishList[index].fishObject);
                fishList.RemoveAt(index);
            }

            yield return new WaitForSeconds(1f);
        }
    }

    Vector3 GetRandomPosition()
    {
        return transform.position + new Vector3(
            UnityEngine.Random.Range(-spawnAreaSize.x / 2f, spawnAreaSize.x / 2f),
            UnityEngine.Random.Range(-spawnAreaSize.y / 2f, spawnAreaSize.y / 2f),
            UnityEngine.Random.Range(-spawnAreaSize.z / 2f, spawnAreaSize.z / 2f)
        );
    }
}
