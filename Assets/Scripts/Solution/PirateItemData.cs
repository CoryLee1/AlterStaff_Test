using UnityEngine;

[System.Serializable]
public class PirateItemData
{
    [Header("Basic Information")]
    public string itemName;            // Internal ID: RoomKey, LifeboatKey, MatchlockGun, etc.
    public string displayName;         // Display name: "Room Key", "Lifeboat Key", "Matchlock Gun", etc.
    [TextArea(2, 4)]
    public string description;         // Item description for UI
    public GameObject prefab;          // Item prefab to spawn in world
    public Sprite itemIcon;           // Item icon for UI (optional)

    [Header("Collection Settings")]
    public int requiredAmount = 1;     // How many of this item need to be collected
    public bool isUnique = true;       // Whether this item is unique (keys are usually unique)

    [Header("Item Classification")]
    public ItemCategory category;      // Item category for organization
    public ItemFunction function;      // What this item does functionally

    [Header("Usage Settings")]
    public bool isUsable = false;      // Can this item be actively used
    public bool isConsumable = false;  // Does this item get consumed when used
    public string targetTag = "";      // Target object tag for interaction

    [Header("Audio")]
    public AudioClip pickupSound;      // Sound when picking up item
    public AudioClip useSound;         // Sound when using item
}

[System.Serializable]
public enum ItemCategory
{
    Key,            // Keys - for opening doors/containers
    Tool,           // Tools - for operating mechanisms  
    Weapon,         // Weapons - for combat/defense
    Treasure,       // Treasures - collectible valuables
    Document,       // Documents - lore and information
    Consumable,     // Consumables - single-use items
    QuestItem       // Quest Items - story progression items
}

[System.Serializable]
public enum ItemFunction
{
    OpenDoor,       // Opens locked doors
    UnlockChest,    // Unlocks treasure chests
    ControlShip,    // Controls ship mechanisms
    DefendSelf,     // Self-defense against enemies
    GetInfo,        // Provides information/lore
    Escape,         // Enables escape options
    Heal,          // Restores health/status
    Currency       // Acts as currency/points
}