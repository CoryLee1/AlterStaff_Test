using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CaptainDialogueSystem : MonoBehaviour
{
    [Header("UI Components")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public GameObject choiceButtonsPanel;
    public UnityEngine.UI.Button askLocationButton;
    public UnityEngine.UI.Button generalTalkButton;
    public UnityEngine.UI.Button endConversationButton;

    [Header("Dialogue Data")]
    public ProgressiveDialogueData dialogueData;

    [Header("Mouse Control")]
    public bool lockMouseAfterDialogue = false; // 设置为false用于prototype

    [Header("Quest Integration")]
    public QuestDashboard questDashboard; // 连接Quest仪表盘

    private string[] currentLines;
    private int index = 0;
    private bool isActive = false;
    private bool showingChoices = false;
    private ItemManager itemManager;

    // 公开isActive供其他脚本检查
    public bool IsActive => isActive;

    void Start()
    {
        itemManager = FindObjectOfType<ItemManager>();

        // Setup button listeners
        if (askLocationButton) askLocationButton.onClick.AddListener(AskAboutLocations);
        if (generalTalkButton) generalTalkButton.onClick.AddListener(GeneralTalk);
        if (endConversationButton) endConversationButton.onClick.AddListener(EndDialogue);
    }

    void Update()
    {
        // 对话中按Enter继续
        if (isActive && !showingChoices && Input.GetKeyDown(KeyCode.Return))
        {
            ShowNextLine();
        }

        // 紧急解锁鼠标（用于调试）
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Debug.Log("Mouse force unlocked with ESC key!");
        }
    }

    public void StartDialogue()
    {
        if (isActive)
        {
            Debug.Log("Dialogue already active, cannot start new dialogue");
            return;
        }

        Debug.Log("Starting dialogue..."); // 调试信息
        isActive = true;
        dialoguePanel.SetActive(true);

        // Show greeting based on collected items
        int collected = itemManager.GetCollectedCount();
        ShowGreeting(collected);
    }

    private void ShowGreeting(int collected)
    {
        currentLines = dialogueData.GetProgressDialogue(collected);
        index = 0;
        showingChoices = false;

        if (currentLines.Length > 0)
        {
            dialogueText.text = currentLines[index];
            choiceButtonsPanel.SetActive(false);
        }
        else
        {
            ShowChoices();
        }

        SetCursorState(true);
    }

    public void AskAboutLocations()
    {
        choiceButtonsPanel.SetActive(false);
        showingChoices = false;

        int collected = itemManager.GetCollectedCount();

        if (collected >= itemManager.GetTotalRequiredCount())
        {
            // All items collected - reveal key location
            string keyLocation = GetKeyLocation();
            currentLines = GetKeyLocationDialogue(keyLocation);
        }
        else
        {
            // Give direct hints about areas
            currentLines = GetLocationHints(collected);
        }

        index = 0;
        if (currentLines.Length > 0)
        {
            dialogueText.text = currentLines[index];
        }
    }

    public void GeneralTalk()
    {
        choiceButtonsPanel.SetActive(false);
        showingChoices = false;

        int collected = itemManager.GetCollectedCount();
        currentLines = dialogueData.GetProgressDialogue(collected);

        index = 0;
        if (currentLines.Length > 0)
        {
            dialogueText.text = currentLines[index];
        }
    }

    private string[] GetLocationHints(int collected)
    {
        List<string> hints = new List<string>();

        switch (collected)
        {
            case 0:
                hints.AddRange(new string[]
                {
                    "You want location hints? Ha!",
                    "Search these areas: CaptainRoom_Center, CaptainRoom_Right_unlocked,",
                    "CaptainRoom_Left_Locked, deckup_Center, deckfront_Center",
                    "That's all you're getting from me!"
                });
                break;

            case 1:
                hints.AddRange(new string[]
                {
                    "You found one item. Fine.",
                    "Try the other captain room areas if you haven't already.",
                    "And don't forget the deck areas: deckup_Center and deckfront_Center.",
                    "Some areas might be locked though..."
                });
                break;

            case 2:
                hints.AddRange(new string[]
                {
                    "Two items... you're getting close.",
                    "Have you checked ALL the CaptainRoom areas?",
                    "CaptainRoom_Center, CaptainRoom_Right_unlocked, CaptainRoom_Left_Locked?",
                    "And both deck areas too?"
                });
                break;

            default:
                string keyLoc = GetKeyLocation();
                hints.AddRange(new string[]
                {
                    "Fine! You found most items!",
                    $"The lifeboat key is in: {keyLoc}",
                    "Now get off my ship!"
                });
                break;
        }

        return hints.ToArray();
    }

    private string GetKeyLocation()
    {
        // Get any key location from ItemManager
        string keyLocation = itemManager.GetKeyLocation("LifeboatKey");

        if (string.IsNullOrEmpty(keyLocation) || keyLocation == "Unknown Location")
        {
            // Try other key names
            foreach (var itemType in itemManager.itemTypes)
            {
                if (itemType.itemName.ToLower().Contains("key"))
                {
                    string location = itemManager.GetKeyLocation(itemType.itemName);
                    if (!string.IsNullOrEmpty(location) && location != "Unknown Location")
                    {
                        return location;
                    }
                }
            }
        }

        return keyLocation ?? "Unknown Location";
    }

    private string[] GetKeyLocationDialogue(string keyLocation)
    {
        return new string[]
        {
            "Alright, alright! You win!",
            $"The lifeboat key is located in: {keyLocation}",
            "Now take it and get out of here!",
            "But you'll never escape my ship alive!"
        };
    }

    public void ShowNextLine()
    {
        index++;
        if (index < currentLines.Length)
        {
            dialogueText.text = currentLines[index];
        }
        else
        {
            ShowChoices();
        }
    }

    private void ShowChoices()
    {
        showingChoices = true;
        choiceButtonsPanel.SetActive(true);

        int collected = itemManager.GetCollectedCount();
        int total = itemManager.GetTotalRequiredCount();

        // Update button text
        if (collected >= total)
        {
            askLocationButton.GetComponentInChildren<TextMeshProUGUI>().text = "Ask about key location";
        }
        else
        {
            askLocationButton.GetComponentInChildren<TextMeshProUGUI>().text = $"Ask about locations ({collected}/{total})";
        }

        dialogueText.text = "What do you want?";
    }

    public void EndDialogue()
    {
        Debug.Log("Ending dialogue..."); // 调试信息

        dialoguePanel.SetActive(false);
        choiceButtonsPanel.SetActive(false);
        isActive = false;
        showingChoices = false;

        // 重置其他状态
        currentLines = null;
        index = 0;

        SetCursorState(false);

        Debug.Log($"Dialogue ended. isActive = {isActive}"); // 确认状态
    }

    private void SetCursorState(bool showCursor)
    {
        if (showCursor)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // Compatibility method
    public void StartDialogueByProgress(int collected)
    {
        StartDialogue();
    }

    // Debug info
    public string GetDebugInfo()
    {
        int collected = itemManager.GetCollectedCount();
        int total = itemManager.GetTotalRequiredCount();
        string keyLocation = GetKeyLocation();

        return $"Items: {collected}/{total}, Key: {keyLocation}";
    }

}