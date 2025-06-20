using System.Collections;
// =====================================================
// ItemRequirementChecker.cs - Checks item requirements for interactions
// =====================================================

using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ItemRequirement
{
    public string itemName;
    public int requiredAmount = 1;
    public bool consumeOnUse = false;
    public string missingItemMessage = "You need this item to proceed";
}

public class ItemRequirementChecker : MonoBehaviour
{
    [Header("Requirements")]
    public List<ItemRequirement> requiredItems = new List<ItemRequirement>();

    [Header("Interaction Settings")]
    public string interactionPrompt = "Press [E] to interact";
    public string successMessage = "Interaction successful!";
    public float interactionRange = 3f;

    [Header("Events")]
    public UnityEngine.Events.UnityEvent OnRequirementsMet;
    public UnityEngine.Events.UnityEvent OnRequirementsNotMet;

    private bool playerInRange = false;
    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (player == null) return;

        CheckPlayerDistance();

        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }

    void CheckPlayerDistance()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        bool wasInRange = playerInRange;
        playerInRange = distance <= interactionRange;

        if (playerInRange && !wasInRange)
        {
            OnPlayerEnterRange();
        }
        else if (!playerInRange && wasInRange)
        {
            OnPlayerExitRange();
        }
    }

    void OnPlayerEnterRange()
    {
        if (CheckAllRequirements())
        {
            PirateItemManager.Instance?.ShowInteractionPrompt(interactionPrompt);
        }
        else
        {
            ShowMissingItemsMessage();
        }
    }

    void OnPlayerExitRange()
    {
        PirateItemManager.Instance?.HideInteractionPrompt();
    }

    void TryInteract()
    {
        if (CheckAllRequirements())
        {
            // Consume items if required
            foreach (var requirement in requiredItems)
            {
                if (requirement.consumeOnUse)
                {
                    ConsumeItem(requirement.itemName, requirement.requiredAmount);
                }
            }

            // Execute success actions
            OnRequirementsMet?.Invoke();
            PirateItemManager.Instance?.ShowInteractionPrompt(successMessage);

            Debug.Log($"Requirements met for {gameObject.name}");
        }
        else
        {
            OnRequirementsNotMet?.Invoke();
            ShowMissingItemsMessage();
        }
    }

    bool CheckAllRequirements()
    {
        if (PirateItemManager.Instance == null) return false;

        foreach (var requirement in requiredItems)
        {
            if (!PirateItemManager.Instance.HasItem(requirement.itemName, requirement.requiredAmount))
            {
                return false;
            }
        }
        return true;
    }

    void ShowMissingItemsMessage()
    {
        var missingItems = GetMissingItems();
        if (missingItems.Count > 0)
        {
            string message = "Missing items: " + string.Join(", ", missingItems);
            PirateItemManager.Instance?.ShowInteractionPrompt(message);
        }
    }

    List<string> GetMissingItems()
    {
        List<string> missing = new List<string>();

        foreach (var requirement in requiredItems)
        {
            if (!PirateItemManager.Instance.HasItem(requirement.itemName, requirement.requiredAmount))
            {
                var itemData = PirateItemManager.Instance.GetItemData(requirement.itemName);
                string displayName = itemData != null ? itemData.displayName : requirement.itemName;
                missing.Add(displayName);
            }
        }

        return missing;
    }

    void ConsumeItem(string itemName, int amount)
    {
        // This would need to be implemented in PirateItemManager
        // For now, just log the consumption
        Debug.Log($"Consumed {amount} of {itemName}");
    }

    // Public method to add requirements dynamically
    public void AddRequirement(string itemName, int amount = 1, bool consumeOnUse = false)
    {
        ItemRequirement newReq = new ItemRequirement
        {
            itemName = itemName,
            requiredAmount = amount,
            consumeOnUse = consumeOnUse
        };
        requiredItems.Add(newReq);
    }

    // Public method to remove requirements
    public void RemoveRequirement(string itemName)
    {
        requiredItems.RemoveAll(req => req.itemName == itemName);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}