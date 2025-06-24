using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class KeyLocation
{
    public string locationId;           // "captainRoom", "kitchen", "deck"
    public string friendlyName;         // "船长室的抽屉里", "厨房的柜子后面"
    public Transform spawnPoint;        // 实际生成位置
    public string dialogueHint;         // 对话中的提示文本
}