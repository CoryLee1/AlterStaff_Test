// =====================================================
// PirateItemManager.cs - Main item management system for pirate ship
// =====================================================

// =====================================================
// PirateItemManager.cs - Main item management system for pirate ship
// =====================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PirateItemManager : MonoBehaviour
{
    [Header("Item Configuration")]
    public List<PirateItemData> allItemTypes = new List<PirateItemData>();

    [Header("Spawn Settings")]
    public List<Transform> spawnPoints = new List<Transform>();
    public bool randomizeSpawnLocations = true;

    [Header("UI Components")]
    public TextMeshProUGUI itemStatusText;
    public TextMeshProUGUI inventoryText;
    public GameObject interactionPrompt;
    public TextMeshProUGUI promptText;

    // Collection tracking
    private Dictionary<string, int> collectedItems = new Dictionary<string, int>();
    private Dictionary<string, bool> unlockedCapabilities = new Dictionary<string, bool>();

    // Singleton pattern
    public static PirateItemManager Instance { get; private set; }

    // Event system
    public System.Action<string, int> OnItemCollected;
    public System.Action<string> OnCapabilityUnlocked;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeItemSystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        SpawnAllItems();
        UpdateAllUI();
    }

    void InitializeItemSystem()
    {
        // Initialize collection status
        foreach (var item in allItemTypes)
        {
            collectedItems[item.itemName] = 0;

            // Initialize capability unlock status
            switch (item.function)
            {
                case ItemFunction.OpenDoor:
                    unlockedCapabilities[$"CanOpen_{item.targetTag}"] = false;
                    break;
                case ItemFunction.DefendSelf:
                    unlockedCapabilities["CanDefend"] = false;
                    break;
                case ItemFunction.Escape:
                    unlockedCapabilities["CanEscape"] = false;
                    break;
            }
        }

        if (interactionPrompt) interactionPrompt.SetActive(false);
    }

    public void ReportCollected(string itemName)
    {
        var itemData = GetItemData(itemName);
        if (itemData == null)
        {
            Debug.LogWarning($"Item not found: {itemName}");
            return;
        }

        // Update collection count
        if (!collectedItems.ContainsKey(itemName))
            collectedItems[itemName] = 0;

        collectedItems[itemName]++;

        // Check for newly unlocked capabilities
        CheckForUnlockedCapabilities(itemData);

        // Trigger events
        OnItemCollected?.Invoke(itemName, collectedItems[itemName]);

        // Update UI
        UpdateAllUI();

        Debug.Log($"Collected: {itemData.displayName} ({collectedItems[itemName]}/{itemData.requiredAmount})");
    }

    void CheckForUnlockedCapabilities(PirateItemData itemData)
    {
        bool hasRequiredAmount = collectedItems[itemData.itemName] >= itemData.requiredAmount;

        if (hasRequiredAmount)
        {
            string capabilityKey = "";

            switch (itemData.function)
            {
                case ItemFunction.OpenDoor:
                    capabilityKey = $"CanOpen_{itemData.targetTag}";
                    break;
                case ItemFunction.DefendSelf:
                    capabilityKey = "CanDefend";
                    break;
                case ItemFunction.Escape:
                    capabilityKey = "CanEscape";
                    break;
                case ItemFunction.UnlockChest:
                    capabilityKey = "CanUnlockChest";
                    break;
            }

            if (!string.IsNullOrEmpty(capabilityKey) && !unlockedCapabilities.ContainsKey(capabilityKey))
            {
                unlockedCapabilities[capabilityKey] = true;
                OnCapabilityUnlocked?.Invoke(capabilityKey);
                Debug.Log($"Unlocked capability: {capabilityKey}");
            }
        }
    }

    void SpawnAllItems()
    {
        List<Transform> availableSpawns = new List<Transform>(spawnPoints);

        foreach (var itemType in allItemTypes)
        {
            for (int i = 0; i < itemType.requiredAmount; i++)
            {
                if (availableSpawns.Count == 0)
                {
                    Debug.LogWarning("Not enough spawn points for all items");
                    break;
                }

                Transform spawnPoint;

                if (randomizeSpawnLocations)
                {
                    // Random spawn
                    int randomIndex = Random.Range(0, availableSpawns.Count);
                    spawnPoint = availableSpawns[randomIndex];
                    availableSpawns.RemoveAt(randomIndex);
                }
                else
                {
                    // Sequential spawn
                    spawnPoint = availableSpawns[0];
                    availableSpawns.RemoveAt(0);
                }

                GameObject spawnedItem = Instantiate(itemType.prefab, spawnPoint.position, spawnPoint.rotation);

                // Setup collectible script
                var collectible = spawnedItem.GetComponent<PirateCollectibleItem>();
                if (collectible == null)
                {
                    collectible = spawnedItem.AddComponent<PirateCollectibleItem>();
                }
                collectible.SetItemData(itemType);

                Debug.Log($"Spawned {itemType.displayName} at {spawnPoint.name}");
            }
        }
    }

    void UpdateAllUI()
    {
        UpdateItemStatusUI();
        UpdateInventoryUI();
    }

    void UpdateItemStatusUI()
    {
        if (itemStatusText == null) return;

        string display = "Collection Progress:\n";

        foreach (var itemType in allItemTypes)
        {
            int collected = collectedItems.ContainsKey(itemType.itemName) ? collectedItems[itemType.itemName] : 0;
            string status = collected >= itemType.requiredAmount ? "✓" : "";
            display += $"{status} {itemType.displayName}: {collected}/{itemType.requiredAmount}\n";
        }

        itemStatusText.text = display;
    }

    void UpdateInventoryUI()
    {
        if (inventoryText == null) return;

        string inventory = "Inventory:\n";

        foreach (var itemType in allItemTypes)
        {
            if (collectedItems.ContainsKey(itemType.itemName) && collectedItems[itemType.itemName] > 0)
            {
                int count = collectedItems[itemType.itemName];
                if (itemType.isUnique)
                {
                    inventory += $"• {itemType.displayName}\n";
                }
                else
                {
                    inventory += $"• {itemType.displayName} x{count}\n";
                }
            }
        }

        if (inventory == "Inventory:\n")
        {
            inventory += "Empty";
        }

        inventoryText.text = inventory;
    }

    // Public query methods
    public bool HasItem(string itemName, int requiredAmount = 1)
    {
        return collectedItems.ContainsKey(itemName) && collectedItems[itemName] >= requiredAmount;
    }

    public bool HasCapability(string capabilityName)
    {
        return unlockedCapabilities.ContainsKey(capabilityName) && unlockedCapabilities[capabilityName];
    }

    public int GetItemCount(string itemName)
    {
        return collectedItems.ContainsKey(itemName) ? collectedItems[itemName] : 0;
    }

    public PirateItemData GetItemData(string itemName)
    {
        foreach (var item in allItemTypes)
        {
            if (item.itemName == itemName)
                return item;
        }
        return null;
    }

    public bool AllRequiredItemsCollected()
    {
        foreach (var itemType in allItemTypes)
        {
            if (GetItemCount(itemType.itemName) < itemType.requiredAmount)
                return false;
        }
        return true;
    }

    public List<string> GetMissingItems()
    {
        List<string> missing = new List<string>();
        foreach (var itemType in allItemTypes)
        {
            if (GetItemCount(itemType.itemName) < itemType.requiredAmount)
            {
                missing.Add(itemType.displayName);
            }
        }
        return missing;
    }

    // UI interaction methods
    public void ShowInteractionPrompt(string message)
    {
        if (interactionPrompt && promptText)
        {
            promptText.text = message;
            interactionPrompt.SetActive(true);
        }
    }

    public void HideInteractionPrompt()
    {
        if (interactionPrompt)
        {
            interactionPrompt.SetActive(false);
        }
    }
}