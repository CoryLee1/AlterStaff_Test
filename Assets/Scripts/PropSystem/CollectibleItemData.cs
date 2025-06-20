using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CollectibleItemData
{
    public string itemName;            // 比如：Key、Gear、Map
    public GameObject prefab;          // 要生成的 Prefab
    public int requiredAmount = 1;     // 玩家要收集几个该类物品
}