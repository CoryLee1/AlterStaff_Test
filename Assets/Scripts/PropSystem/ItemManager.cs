using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Linq;

public class ItemManager : MonoBehaviour
{
    [Header("Item Configuration")]
    public List<CollectibleItemData> itemTypes;
    public TextMeshProUGUI itemStatusText;

    [Header("Room Areas")]
    public List<RoomArea> roomAreas;

    [Header("Debug UI")]
    public TextMeshProUGUI debugKeyLocationText;  // Debug UI showing key locations
    public bool showDebugInfo = true;             // Whether to show debug information

    [Header("Spawn Settings")]
    public float spawnHeight = 0.1f;
    public int maxAttempts = 50;      // Max attempts to avoid item overlap

    private Dictionary<string, int> collected = new Dictionary<string, int>();
    private Dictionary<string, string> keyLocationAssignments = new Dictionary<string, string>(); // Key->Room assignments
                                                                                                  // 公开collected字典供QuestDashboard访问
    public Dictionary<string, int> Collected => collected;

    [System.Serializable]
    public class RoomArea
    {
        [Header("Basic Info")]
        public string roomName;
        public BoxCollider areaBounds;

        [Header("Spawn Settings")]
        public int minItems = 0;           // Not including assigned keys
        public int maxItems = 2;           // Not including assigned keys
        [Range(0f, 1f)]
        public float spawnChance = 0.8f;

        [Header("Key Assignment")]
        public bool canHaveKeys = true;    // Whether this room can be assigned keys

        [Header("Debug")]
        public Color gizmoColor = Color.yellow;
        public bool showGizmo = true;

        public Vector3 Center => areaBounds != null ? areaBounds.bounds.center : Vector3.zero;
        public Vector3 Size => areaBounds != null ? areaBounds.bounds.size : Vector3.zero;
    }

    void Start()
    {
        RandomlyAssignKeyLocations();
        SpawnAllItems();
        UpdateUI();
        UpdateDebugUI();
    }

    /// <summary>
    /// Randomly assign keys to different rooms
    /// </summary>
    private void RandomlyAssignKeyLocations()
    {
        keyLocationAssignments.Clear();

        // Get all key types (identified by name or special marking)
        var keyItems = itemTypes.Where(item =>
            item.itemName.ToLower().Contains("key") ||
            item.itemName.ToLower().Contains("钥匙")).ToList();

        // Get rooms that can have keys
        var availableRooms = roomAreas.Where(room => room.canHaveKeys).ToList();

        if (keyItems.Count > availableRooms.Count)
        {
            Debug.LogWarning($"Key count ({keyItems.Count}) exceeds available room count ({availableRooms.Count})!");
        }

        // Randomly shuffle room order
        availableRooms = availableRooms.OrderBy(x => UnityEngine.Random.value).ToList();

        // Assign each key to a room
        for (int i = 0; i < keyItems.Count && i < availableRooms.Count; i++)
        {
            string keyName = keyItems[i].itemName;
            string roomName = availableRooms[i].roomName;

            keyLocationAssignments[keyName] = roomName;

            Debug.Log($"🗝️ {keyName} assigned to → {roomName}");
        }
    }

    private void SpawnAllItems()
    {
        // 1. First spawn assigned keys
        SpawnAssignedKeys();

        // 2. Spawn other items
        SpawnRegularItems();

        // 3. Ensure minimum item count requirements are met
        EnsureMinimumItemCounts();
    }

    /// <summary>
    /// Spawn assigned keys
    /// </summary>
    private void SpawnAssignedKeys()
    {
        foreach (var assignment in keyLocationAssignments)
        {
            string keyName = assignment.Key;
            string roomName = assignment.Value;

            // Find key data
            var keyItemData = itemTypes.FirstOrDefault(item => item.itemName == keyName);
            if (keyItemData == null) continue;

            // Find target room
            var targetRoom = roomAreas.FirstOrDefault(room => room.roomName == roomName);
            if (targetRoom == null) continue;

            // Spawn key in target room
            Vector3 spawnPos = GetRandomPositionInArea(targetRoom);
            if (spawnPos != Vector3.zero)
            {
                GameObject key = Instantiate(keyItemData.prefab, spawnPos, GetRandomRotation());
                var collectible = key.GetComponent<CollectibleItem>();
                if (collectible != null)
                {
                    collectible.itemName = keyItemData.itemName;
                }
                key.name = $"{keyName}_{roomName}_Assigned";

                Debug.Log($"✅ Spawned key {keyName} in {roomName}");
            }
        }
    }

    /// <summary>
    /// Spawn other random items
    /// </summary>
    private void SpawnRegularItems()
    {
        foreach (var room in roomAreas)
        {
            if (UnityEngine.Random.value > room.spawnChance) continue;

            int itemCount = UnityEngine.Random.Range(room.minItems, room.maxItems + 1);

            for (int i = 0; i < itemCount; i++)
            {
                // Select non-key items, or keys not assigned to this room
                var availableItems = GetAvailableItemsForRoom(room);
                if (availableItems.Count == 0) continue;

                var selectedItem = availableItems[UnityEngine.Random.Range(0, availableItems.Count)];

                Vector3 spawnPos = GetRandomPositionInArea(room);
                if (spawnPos != Vector3.zero)
                {
                    GameObject item = Instantiate(selectedItem.prefab, spawnPos, GetRandomRotation());
                    var collectible = item.GetComponent<CollectibleItem>();
                    if (collectible != null)
                    {
                        collectible.itemName = selectedItem.itemName;
                    }
                    item.name = $"{selectedItem.itemName}_{room.roomName}_{i}";

                    Debug.Log($"📦 Spawned {selectedItem.itemName} in {room.roomName}");
                }
            }
        }
    }

    /// <summary>
    /// Get available items for room (excluding keys assigned to other rooms)
    /// </summary>
    private List<CollectibleItemData> GetAvailableItemsForRoom(RoomArea room)
    {
        var availableItems = new List<CollectibleItemData>();

        foreach (var itemData in itemTypes)
        {
            // If this is a key, check if it's assigned to another room
            if (keyLocationAssignments.ContainsKey(itemData.itemName))
            {
                // Only keys assigned to current room can spawn additional copies (to meet required amount)
                if (keyLocationAssignments[itemData.itemName] == room.roomName)
                {
                    // Check if this key needs more copies
                    int currentCount = GameObject.FindGameObjectsWithTag("Item")
                        .Count(go => go.GetComponent<CollectibleItem>()?.itemName == itemData.itemName);

                    if (currentCount < itemData.requiredAmount)
                    {
                        availableItems.Add(itemData);
                    }
                }
                // If key is assigned to another room, skip
                continue;
            }

            // Non-key items can always spawn
            availableItems.Add(itemData);
        }

        return availableItems;
    }

    private void EnsureMinimumItemCounts()
    {
        foreach (var itemData in itemTypes)
        {
            int currentCount = GameObject.FindGameObjectsWithTag("Item")
                .Count(go => go.GetComponent<CollectibleItem>()?.itemName == itemData.itemName);

            int needed = itemData.requiredAmount - currentCount;

            for (int i = 0; i < needed; i++)
            {
                RoomArea targetRoom = null;

                // If it's a key, spawn in assigned room
                if (keyLocationAssignments.ContainsKey(itemData.itemName))
                {
                    string assignedRoomName = keyLocationAssignments[itemData.itemName];
                    targetRoom = roomAreas.FirstOrDefault(room => room.roomName == assignedRoomName);
                }
                else
                {
                    // If not a key, randomly select a room
                    var availableRooms = roomAreas.ToList();
                    targetRoom = availableRooms[UnityEngine.Random.Range(0, availableRooms.Count)];
                }

                if (targetRoom != null)
                {
                    Vector3 spawnPos = GetRandomPositionInArea(targetRoom);
                    if (spawnPos != Vector3.zero)
                    {
                        GameObject item = Instantiate(itemData.prefab, spawnPos, GetRandomRotation());
                        var collectible = item.GetComponent<CollectibleItem>();
                        if (collectible != null)
                        {
                            collectible.itemName = itemData.itemName;
                        }
                        item.name = $"{itemData.itemName}_{targetRoom.roomName}_Extra";

                        Debug.Log($"🔄 Additional spawn {itemData.itemName} in {targetRoom.roomName}");
                    }
                }
            }
        }
    }

    private Vector3 GetRandomPositionInArea(RoomArea room)
    {
        if (room.areaBounds == null)
        {
            Debug.LogWarning($"Room {room.roomName} has no BoxCollider set");
            return Vector3.zero;
        }

        Bounds bounds = room.areaBounds.bounds;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            // Generate random position directly within BoxCollider bounds
            Vector3 randomPosition = new Vector3(
                UnityEngine.Random.Range(bounds.min.x, bounds.max.x),
                bounds.min.y + spawnHeight, // At BoxCollider bottom + slight height
                UnityEngine.Random.Range(bounds.min.z, bounds.max.z)
            );

            // Check if position has enough space (avoid item overlap)
            if (!Physics.CheckSphere(randomPosition, 0.3f))
            {
                Debug.Log($"✅ Successfully found spawn position in {room.roomName}: {randomPosition}");
                return randomPosition;
            }
        }

        // Fallback position
        Debug.LogWarning($"❌ After {maxAttempts} attempts in {room.roomName}, no free space found, using center position");
        return new Vector3(bounds.center.x, bounds.min.y + spawnHeight, bounds.center.z);
    }

    private Quaternion GetRandomRotation()
    {
        return Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);
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

    /// <summary>
    /// Update debug UI showing key distribution
    /// </summary>
    private void UpdateDebugUI()
    {
        if (debugKeyLocationText == null || !showDebugInfo) return;

        string debugText = "🗝️ Debug Console Use：Key Location Distribution:\n";
        foreach (var assignment in keyLocationAssignments)
        {
            debugText += $"• {assignment.Key} → {assignment.Value}\n";
        }

        debugKeyLocationText.text = debugText;
    }

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

    /// <summary>
    /// Check if specific item has been collected in sufficient quantity
    /// </summary>
    public bool IsItemCollected(string itemName)
    {
        var itemData = itemTypes.FirstOrDefault(item => item.itemName == itemName);
        if (itemData == null) return false;

        int collectedCount = collected.ContainsKey(itemName) ? collected[itemName] : 0;
        return collectedCount >= itemData.requiredAmount;
    }

    /// <summary>
    /// Get total required item count
    /// </summary>
    public int GetTotalRequiredCount()
    {
        int total = 0;
        foreach (var item in itemTypes)
        {
            total += item.requiredAmount;
        }
        return total;
    }

    /// <summary>
    /// Get collection progress for specific item
    /// </summary>
    public string GetItemProgress(string itemName)
    {
        var itemData = itemTypes.FirstOrDefault(item => item.itemName == itemName);
        if (itemData == null) return "Item not found";

        int collectedCount = collected.ContainsKey(itemName) ? collected[itemName] : 0;
        return $"{collectedCount}/{itemData.requiredAmount}";
    }

    [ContextMenu("Clear and Respawn Items")]
    public void ClearAndRespawnItems()
    {
        GameObject[] existingItems = GameObject.FindGameObjectsWithTag("Item");
        foreach (GameObject item in existingItems)
        {
            if (Application.isPlaying)
                Destroy(item);
            else
                DestroyImmediate(item);
        }

        RandomlyAssignKeyLocations();
        SpawnAllItems();
        UpdateDebugUI();
    }

    // Public method: Get key location (for other scripts to call)
    public string GetKeyLocation(string keyName)
    {
        return keyLocationAssignments.ContainsKey(keyName) ? keyLocationAssignments[keyName] : "Unknown Location";
    }

    // Compatibility method: Maintain compatibility with old GameManager
    [System.Obsolete("This method is no longer needed. Key assignment is handled automatically in Start().")]
    public void InitializeLifeboatKey()
    {
        Debug.Log("InitializeLifeboatKey() is deprecated. Key assignment is handled automatically.");
        // Empty method to avoid errors but perform no operations
    }

    // Editor visualization
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (roomAreas == null) return;

        foreach (var room in roomAreas)
        {
            if (!room.showGizmo || room.areaBounds == null) continue;

            Bounds bounds = room.areaBounds.bounds;

            Gizmos.color = room.gizmoColor;
            Gizmos.DrawWireCube(bounds.center, bounds.size);

            // Display room name and assigned keys (if any)
            string label = room.roomName;
            if (Application.isPlaying && keyLocationAssignments != null)
            {
                var assignedKeys = keyLocationAssignments.Where(kv => kv.Value == room.roomName)
                    .Select(kv => kv.Key).ToArray();
                if (assignedKeys.Length > 0)
                {
                    label += $"\n🗝️ {string.Join(", ", assignedKeys)}";
                }
            }

            UnityEditor.Handles.Label(
                bounds.center + Vector3.up * (bounds.size.y / 2 + 0.5f),
                label
            );

            // If can have keys, mark with cyan border
            if (room.canHaveKeys)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(bounds.center, bounds.size * 1.02f);
            }
        }
    }
#endif
}