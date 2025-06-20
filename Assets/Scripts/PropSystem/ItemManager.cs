using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class ItemManager : MonoBehaviour
{
    public List<CollectibleItemData> itemTypes;
    public List<Transform> spawnPoints; // 场景中放置的空物体作为可能的生成位置
    public TextMeshProUGUI itemStatusText;

    private Dictionary<string, int> collected = new Dictionary<string, int>();

    void Start()
    {
        SpawnAllItems();
        UpdateUI();
    }

    public void ReportCollected(string itemName)
    {
        if (!collected.ContainsKey(itemName))
            collected[itemName] = 0;

        collected[itemName]++;
        UpdateUI();
    }

    private void UpdateUI()
    {
        string display = "";
        foreach (var item in itemTypes)
        {
            int count = collected.ContainsKey(item.itemName) ? collected[item.itemName] : 0;
            display += $"{item.itemName}: {count}/{item.requiredAmount}\n";
        }
        itemStatusText.text = display;
    }

    private void SpawnAllItems()
    {
        List<Transform> available = new List<Transform>(spawnPoints);
        foreach (var item in itemTypes)
        {
            for (int i = 0; i < item.requiredAmount; i++)
            {
                if (available.Count == 0) break;

                int index = UnityEngine.Random.Range(0, available.Count);
                Transform spawn = available[index];
                available.RemoveAt(index);

                Instantiate(item.prefab, spawn.position, Quaternion.identity);
            }
        }
    }
    //Endpoint to get the number of props
    public int GetCollectedCount()
    {
        int count = 0;
        foreach (var item in itemTypes)
        {
            count += collected.ContainsKey(item.itemName) ? collected[item.itemName] : 0;
        }
        return count;
    }

    public bool AllItemsCollected()
    {
        foreach (var item in itemTypes)
        {
            int count = collected.ContainsKey(item.itemName) ? collected[item.itemName] : 0;
            if (count < item.requiredAmount)
                return false;
        }
        return true;
    }

}