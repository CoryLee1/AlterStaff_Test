using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CollectibleItemData
{
    public string itemName;            // 比如：Key、Gear、Map
    public GameObject prefab;          // 要生成的 Prefab
    public int requiredAmount = 1;     // 玩家要收集几个该类物品
                                       // 新增：用于LifeboatKey的位置描述
    public string locationDescription = ""; // 如"船长室的抽屉里"
    public bool isLifeboatKey = false;      // 标记这是否是救生艇钥匙
}