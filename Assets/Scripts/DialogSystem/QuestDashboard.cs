using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class QuestDashboard : MonoBehaviour
{
    [Header("UI Components")]
    public GameObject dashboardPanel;
    public TextMeshProUGUI questTitleText;
    public TextMeshProUGUI itemProgressText;
    public TextMeshProUGUI cluesText;
    public TextMeshProUGUI objectiveText;
    public Button toggleButton;

    [Header("Settings")]
    public KeyCode toggleKey = KeyCode.Tab;
    public bool startVisible = false;

    private ItemManager itemManager;
    private List<string> discoveredClues = new List<string>();
    private bool isVisible = false;

    void Start()
    {
        itemManager = FindObjectOfType<ItemManager>();

        // Setup button
        if (toggleButton)
            toggleButton.onClick.AddListener(ToggleDashboard);

        // Initial state
        isVisible = startVisible;
        dashboardPanel.SetActive(isVisible);

        // Initial update
        UpdateDashboard();

        // Update every second
        InvokeRepeating(nameof(UpdateDashboard), 1f, 1f);
    }

    void Update()
    {
        // Toggle with key
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleDashboard();
        }
    }

    public void ToggleDashboard()
    {
        isVisible = !isVisible;
        dashboardPanel.SetActive(isVisible);

        if (isVisible)
        {
            UpdateDashboard();
        }
    }

    void UpdateDashboard()
    {
        if (!isVisible) return;

        UpdateQuestTitle();
        UpdateItemProgress();
        UpdateObjective();
        UpdateClues();
    }

    void UpdateQuestTitle()
    {
        if (questTitleText)
        {
            questTitleText.text = "ESCAPE THE SHIP";
        }
    }

    void UpdateItemProgress()
    {
        if (itemProgressText && itemManager)
        {
            int collected = itemManager.GetCollectedCount();
            int total = itemManager.GetTotalRequiredCount();

            string progressText = "ITEM COLLECTION PROGRESS:\n";
            progressText += $"Total Items: {collected}/{total}\n\n";

            // Show individual item progress
            foreach (var itemType in itemManager.itemTypes)
            {
                int itemCount = 0;
                if (itemManager.Collected.ContainsKey(itemType.itemName)) // 使用公共属性
                {
                    itemCount = itemManager.Collected[itemType.itemName];
                }

                string status = itemCount >= itemType.requiredAmount ? "[COMPLETE]" : "[NEEDED]";
                progressText += $"• {itemType.itemName}: {itemCount}/{itemType.requiredAmount} {status}\n";
            }

            itemProgressText.text = progressText;
        }
    }

    void UpdateObjective()
    {
        if (objectiveText && itemManager)
        {
            int collected = itemManager.GetCollectedCount();
            int total = itemManager.GetTotalRequiredCount();

            string objective = "CURRENT OBJECTIVE:\n";

            if (collected == 0)
            {
                objective += "• Explore the ship and find hidden items\n";
                objective += "• Talk to the Captain for hints\n";
                objective += "• Search all rooms carefully";
            }
            else if (collected < total)
            {
                objective += $"• Find {total - collected} more items\n";
                objective += "• Ask the Captain about locations\n";
                objective += "• Check areas you haven't explored yet";
            }
            else
            {
                objective += "• All items collected!\n";
                objective += "• Ask Captain about lifeboat key location\n";
                objective += "• Find the key and escape!";
            }

            objectiveText.text = objective;
        }
    }

    void UpdateClues()
    {
        if (cluesText)
        {
            string cluesDisplay = "DISCOVERED CLUES:\n";

            if (discoveredClues.Count == 0)
            {
                cluesDisplay += "• No clues discovered yet\n";
                cluesDisplay += "• Talk to the Captain to get hints";
            }
            else
            {
                foreach (string clue in discoveredClues)
                {
                    cluesDisplay += $"• {clue}\n";
                }
            }

            cluesText.text = cluesDisplay;
        }
    }

    // Public methods to add clues from other systems
    public void AddClue(string clue)
    {
        if (!discoveredClues.Contains(clue))
        {
            discoveredClues.Add(clue);
            Debug.Log($"New clue added: {clue}");

            // Show notification
            StartCoroutine(ShowClueNotification(clue));
        }
    }

    public void AddLocationClue(string location)
    {
        string clue = $"Captain mentioned: Check {location}";
        AddClue(clue);
    }

    public void AddKeyLocationClue(string keyLocation)
    {
        string clue = $"Lifeboat key is hidden {keyLocation}";
        AddClue(clue);
    }

    public void AddGeneralClue(string clueText)
    {
        AddClue(clueText);
    }

    // Show a temporary notification when clue is added
    IEnumerator ShowClueNotification(string clue)
    {
        // Create temporary notification (you can make this fancier)
        GameObject notification = new GameObject("ClueNotification");
        notification.transform.SetParent(transform);

        TextMeshProUGUI notifText = notification.AddComponent<TextMeshProUGUI>();
        notifText.text = $"NEW CLUE: {clue}";
        notifText.fontSize = 18;
        notifText.color = Color.yellow;

        RectTransform rectTransform = notification.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(0, 100);

        // Fade in and out
        float timer = 0f;
        while (timer < 3f)
        {
            timer += Time.deltaTime;
            float alpha = timer < 1f ? timer : (3f - timer) / 2f;
            notifText.color = new Color(1f, 1f, 0f, alpha);
            yield return null;
        }

        Destroy(notification);
    }

    // Integration with dialogue system
    public void OnDialogueClueReceived(int collectedCount, string specificClue = "")
    {
        switch (collectedCount)
        {
            case 0:
                AddClue("Captain refuses to give hints");
                AddClue("Search areas where a captain spends time");
                break;
            case 1:
                AddClue("Captain suggests checking important captain areas");
                AddClue("Look in quarters and command areas");
                break;
            case 2:
                AddClue("Captain hints at deck areas and unexpected places");
                AddClue("Some treasures are well hidden");
                break;
            default:
                if (!string.IsNullOrEmpty(specificClue))
                {
                    AddKeyLocationClue(specificClue);
                }
                break;
        }
    }

    // Get current quest state for other systems
    public QuestState GetQuestState()
    {
        int collected = itemManager ? itemManager.GetCollectedCount() : 0;
        int total = itemManager ? itemManager.GetTotalRequiredCount() : 0;

        return new QuestState
        {
            itemsCollected = collected,
            totalItems = total,
            cluesDiscovered = discoveredClues.Count,
            allItemsFound = collected >= total,
            discoveredClues = new List<string>(discoveredClues)
        };
    }

    [System.Serializable]
    public class QuestState
    {
        public int itemsCollected;
        public int totalItems;
        public int cluesDiscovered;
        public bool allItemsFound;
        public List<string> discoveredClues;
    }
}