// =====================================================
// BlueParrotDialogue.cs - Simple parrot dialogue system
// =====================================================

using UnityEngine;
using TMPro;

public class BlueParrotDialogue : MonoBehaviour
{
    [Header("Dialogue UI")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI promptText;  // "Press T to talk" prompt

    [Header("Dialogue Content")]
    [TextArea(3, 5)]
    public string[] normalDialogue = {
        "Squawk! Hello there, sailor!",
        "The captain loves her treasures very much.",
        "She keeps everything locked up tight!",
        "Squawk!"
    };

    [TextArea(3, 5)]
    public string[] treasureHintDialogue = {
        "Squawk! Looking for something?",
        "The captain's most precious treasure is in that chest!",
        "But it's locked with her favorite numbers...",
        "She always says 7-3-9 brings her luck!",
        "Don't tell her I told you! Squawk!"
    };

    [Header("Interaction Settings")]
    public float interactionRange = 3f;
    public KeyCode talkKey = KeyCode.T;

    private string[] currentDialogue;
    private int dialogueIndex = 0;
    private bool isInDialogue = false;
    private bool playerInRange = false;
    private bool hasGivenHint = false;
    private Transform player;

    // Static password for treasure chest
    public static string TreasurePassword = "739";

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (dialoguePanel) dialoguePanel.SetActive(false);
        if (promptText) promptText.gameObject.SetActive(false);
    }

    void Update()
    {
        CheckPlayerDistance();

        if (playerInRange && Input.GetKeyDown(talkKey) && !isInDialogue)
        {
            StartDialogue();
        }

        if (isInDialogue && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
        {
            ShowNextLine();
        }
    }

    void CheckPlayerDistance()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        bool wasInRange = playerInRange;
        playerInRange = distance <= interactionRange;

        if (playerInRange && !wasInRange && !isInDialogue)
        {
            ShowTalkPrompt();
        }
        else if (!playerInRange && wasInRange)
        {
            HideTalkPrompt();
        }
    }

    void ShowTalkPrompt()
    {
        if (promptText)
        {
            promptText.text = "Press [T] to talk to parrot";
            promptText.gameObject.SetActive(true);
        }
    }

    void HideTalkPrompt()
    {
        if (promptText)
        {
            promptText.gameObject.SetActive(false);
        }
    }

    void StartDialogue()
    {
        // Check if player is examining treasure chest to give hint
        bool shouldGiveHint = ShouldGiveTreasureHint();

        if (shouldGiveHint && !hasGivenHint)
        {
            currentDialogue = treasureHintDialogue;
            hasGivenHint = true;
            Debug.Log("Parrot gives treasure chest hint!");
        }
        else
        {
            currentDialogue = normalDialogue;
        }

        dialogueIndex = 0;
        isInDialogue = true;

        if (dialoguePanel) dialoguePanel.SetActive(true);
        if (promptText) promptText.gameObject.SetActive(false);

        ShowCurrentLine();

        // Pause game and show cursor
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void ShowCurrentLine()
    {
        if (dialogueText && dialogueIndex < currentDialogue.Length)
        {
            dialogueText.text = currentDialogue[dialogueIndex];
        }
    }

    void ShowNextLine()
    {
        dialogueIndex++;

        if (dialogueIndex < currentDialogue.Length)
        {
            ShowCurrentLine();
        }
        else
        {
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        isInDialogue = false;

        if (dialoguePanel) dialoguePanel.SetActive(false);

        // Resume game
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Show talk prompt again if player still in range
        if (playerInRange)
        {
            ShowTalkPrompt();
        }
    }

    bool ShouldGiveTreasureHint()
    {
        // Check if player is near treasure chest or has been examining captain's room
        var treasureChest = FindObjectOfType<TreasureChestLock>();
        if (treasureChest != null)
        {
            float distanceToChest = Vector3.Distance(player.position, treasureChest.transform.position);
            if (distanceToChest < 5f) // Player is near treasure chest
            {
                return true;
            }
        }

        // Alternative: Check if player has certain items (suggesting they've been exploring)
        var itemManager = FindObjectOfType<PirateItemManager>();
        if (itemManager != null)
        {
            // If player has collected some items, they're exploring
            return itemManager.GetItemCount("RoomKey") > 0 || itemManager.AllRequiredItemsCollected();
        }

        return false;
    }

    // Public method to force treasure hint (can be called by other scripts)
    public void TriggerTreasureHint()
    {
        if (!hasGivenHint && playerInRange && !isInDialogue)
        {
            currentDialogue = treasureHintDialogue;
            hasGivenHint = true;
            StartDialogue();
        }
    }

    // Public method to check if hint was given
    public bool HasGivenTreasureHint()
    {
        return hasGivenHint;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}

