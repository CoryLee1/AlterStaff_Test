using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TreasureChestLock : MonoBehaviour
{
    [Header("Lock UI Components")]
    public GameObject lockPanel;
    public TMP_Dropdown[] digitDropdowns = new TMP_Dropdown[3]; // TextMeshPro Dropdowns
    public Button submitButton;      // Button component (with TextMeshPro text inside)
    public Button closeButton;       // Button component (with TextMeshPro text inside)
    public TextMeshProUGUI feedbackText;    // TextMeshPro text
    public TextMeshProUGUI titleText;       // TextMeshPro title text

    [Header("Interaction UI")]
    public GameObject interactionPromptUI;   // UI Panel for interaction prompt
    public TextMeshProUGUI interactionPromptText; // "Press [E] to open combination lock"

    [Header("Chest Settings")]
    public Animator chestAnimator;
    public string correctCombination = "739"; // Password from parrot
    public GameObject chestContents;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip unlockSound;
    public AudioClip wrongCodeSound;
    public AudioClip clickSound;

    [Header("Visual Effects")]
    public ParticleSystem unlockEffect;

    [Header("Interaction")]
    public float interactionRange = 3f;
    public string interactionPrompt = "Press [E] to open combination lock";
    public string needPasswordPrompt = "The chest is locked. Maybe someone knows the combination...";

    [Header("UI Text Content")]
    public string lockTitle = "Enter Combination";
    public string submitButtonText = "SUBMIT";
    public string closeButtonText = "CLOSE";
    public string enterCombinationText = "Enter the combination:";
    public string wrongCombinationText = "Wrong combination! Try again.";

    private bool isUnlocked = false;
    private bool passwordKnown = false;
    private Transform player;
    private bool playerInRange = false;

    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null) player = playerObject.transform;

        if (lockPanel) lockPanel.SetActive(false);
        if (chestContents) chestContents.SetActive(false);
        if (interactionPromptUI) interactionPromptUI.SetActive(false); // Hide prompt initially

        SetupDropdowns();
        SetupButtons();
        SetupUIText();

        // Get password from parrot
        correctCombination = BlueParrotDialogue.TreasurePassword;
    }

    void Update()
    {
        CheckPlayerDistance();

        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (isUnlocked)
            {
                ShowUnlockedMessage();
            }
            else
            {
                // Always allow opening lock interface, regardless of password knowledge
                OpenLockInterface();
            }
        }
    }

    void CheckPlayerDistance()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        bool wasInRange = playerInRange;
        playerInRange = distance <= interactionRange;

        if (playerInRange && !wasInRange)
        {
            ShowInteractionPrompt();
        }
        else if (!playerInRange && wasInRange)
        {
            HideInteractionPrompt();
        }
    }

    void ShowInteractionPrompt()
    {
        string promptMessage = "";

        if (isUnlocked)
        {
            promptMessage = "Chest is already open";
        }
        else
        {
            promptMessage = interactionPrompt; // "Press [E] to open combination lock"
        }

        // Show UI prompt
        if (interactionPromptUI && interactionPromptText)
        {
            interactionPromptText.text = promptMessage;
            interactionPromptUI.SetActive(true);
        }

        // Also log to console for debugging
        Debug.Log(promptMessage);
    }

    void HideInteractionPrompt()
    {
        // Hide UI prompt
        if (interactionPromptUI)
        {
            interactionPromptUI.SetActive(false);
        }
    }

    void SetupDropdowns()
    {
        for (int i = 0; i < digitDropdowns.Length; i++)
        {
            if (digitDropdowns[i] != null)
            {
                // Clear existing options
                digitDropdowns[i].ClearOptions();

                // Create list of options (0-9)
                var options = new System.Collections.Generic.List<string>();
                for (int digit = 0; digit <= 9; digit++)
                {
                    options.Add(digit.ToString());
                }

                // Add options to dropdown
                digitDropdowns[i].AddOptions(options);

                // Set default value to 0
                digitDropdowns[i].value = 0;

                // Refresh dropdown display
                digitDropdowns[i].RefreshShownValue();

                Debug.Log($"Dropdown {i} setup with options 0-9");
            }
        }
    }

    void SetupButtons()
    {
        if (submitButton)
        {
            submitButton.onClick.AddListener(TryUnlock);

            // Set TextMeshPro text for submit button
            TextMeshProUGUI submitText = submitButton.GetComponentInChildren<TextMeshProUGUI>();
            if (submitText) submitText.text = submitButtonText;
        }

        if (closeButton)
        {
            closeButton.onClick.AddListener(CloseLockInterface);

            // Set TextMeshPro text for close button
            TextMeshProUGUI closeText = closeButton.GetComponentInChildren<TextMeshProUGUI>();
            if (closeText) closeText.text = closeButtonText;
        }
    }

    void SetupUIText()
    {
        if (titleText)
        {
            titleText.text = lockTitle;
        }

        if (feedbackText)
        {
            feedbackText.text = enterCombinationText;
        }
    }

    public void SetPasswordKnown(bool known)
    {
        passwordKnown = known;
        Debug.Log("Password revealed by parrot!");
    }

    void OpenLockInterface()
    {
        // Hide interaction prompt when opening lock
        HideInteractionPrompt();

        if (lockPanel)
        {
            lockPanel.SetActive(true);
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Show different feedback based on password knowledge
        if (feedbackText)
        {
            if (passwordKnown)
            {
                feedbackText.text = enterCombinationText; // "Enter the combination:"
            }
            else
            {
                feedbackText.text = "I need to find out the combination... Maybe someone knows it?";
            }
        }
    }

    public void CloseLockInterface()
    {
        if (lockPanel)
        {
            lockPanel.SetActive(false);
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // Show interaction prompt again if player still in range
        if (playerInRange)
        {
            ShowInteractionPrompt();
        }

        PlayClickSound();
    }

    public void TryUnlock()
    {
        string enteredCode = "";

        for (int i = 0; i < digitDropdowns.Length; i++)
        {
            if (digitDropdowns[i] != null)
            {
                enteredCode += digitDropdowns[i].options[digitDropdowns[i].value].text;
            }
        }

        Debug.Log("Entered code: " + enteredCode + ", Correct: " + correctCombination);

        if (enteredCode == correctCombination)
        {
            UnlockChest();
        }
        else
        {
            ShowWrongCode();

            // Give hint about talking to parrot if password not known
            if (!passwordKnown)
            {
                ShowParrotHint();
            }
        }

        PlayClickSound();
    }

    void ShowParrotHint()
    {
        if (feedbackText)
        {
            feedbackText.text = "Wrong combination! Maybe I should talk to someone who knows the captain...";
        }
    }

    void UnlockChest()
    {
        isUnlocked = true;
        CloseLockInterface();

        // Play unlock animation
        if (chestAnimator)
        {
            chestAnimator.SetTrigger("Open");
        }

        // Play unlock sound
        if (audioSource && unlockSound)
        {
            audioSource.PlayOneShot(unlockSound);
        }

        // Show unlock effect
        if (unlockEffect)
        {
            unlockEffect.Play();
        }

        // Show chest contents
        if (chestContents)
        {
            chestContents.SetActive(true);
        }

        Debug.Log("Treasure chest unlocked!");

        // Add items to inventory
        var itemManager = FindObjectOfType<PirateItemManager>();
        if (itemManager)
        {
            itemManager.ReportCollected("MatchlockGun");
        }
    }

    void ShowWrongCode()
    {
        if (feedbackText)
        {
            feedbackText.text = wrongCombinationText;
        }

        if (audioSource && wrongCodeSound)
        {
            audioSource.PlayOneShot(wrongCodeSound);
        }

        // Reset feedback text after delay
        StartCoroutine(ResetFeedbackText());
    }

    System.Collections.IEnumerator ResetFeedbackText()
    {
        yield return new WaitForSecondsRealtime(2f);
        if (feedbackText)
        {
            feedbackText.text = enterCombinationText;
        }
    }

    void ShowNeedPasswordMessage()
    {
        Debug.Log("I need to talk to someone who might know the combination...");

        // Check if parrot has given hint, if so, unlock password knowledge
        var parrot = FindObjectOfType<BlueParrotDialogue>();
        if (parrot != null && parrot.HasGivenTreasureHint())
        {
            SetPasswordKnown(true);
            Debug.Log("Now I know the combination! Let me try opening it.");
        }
    }

    void ShowUnlockedMessage()
    {
        Debug.Log("The treasure chest is already open.");
    }

    void PlayClickSound()
    {
        if (audioSource && clickSound)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}