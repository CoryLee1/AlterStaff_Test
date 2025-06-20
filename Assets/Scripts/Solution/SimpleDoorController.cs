// =====================================================
// SimpleDoorController.cs - Simple door rotation controller
// =====================================================

using UnityEngine;
using System.Collections;

public class SimpleDoorController : MonoBehaviour
{
    [Header("Door Settings")]
    public bool isLocked = true;
    public string requiredItemName = "RoomKey";
    public float openAngle = 90f;           // How much to rotate when opening
    public float doorSpeed = 2f;            // Speed of door opening/closing
    public bool closeAutomatically = false; // Whether door closes automatically
    public float autoCloseDelay = 3f;       // Time before auto-close

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip unlockSound;
    public AudioClip openSound;
    public AudioClip closeSound;
    public AudioClip lockedSound;

    [Header("Interaction")]
    public float interactionRange = 3f;
    public string interactionPrompt = "Press [E] to open door";
    public string lockedPrompt = "Door is locked - need Room Key";

    private bool isOpen = false;
    private bool isMoving = false;
    private Vector3 closedRotation;
    private Vector3 openRotation;
    private Transform player;
    private bool playerInRange = false;
    private Coroutine autoCloseCoroutine;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        // Store initial rotation as closed position
        closedRotation = transform.eulerAngles;
        // Calculate open rotation (add 90 degrees to Y axis)
        openRotation = closedRotation + new Vector3(0, openAngle, 0);

        Debug.Log($"Door {gameObject.name} - Closed: {closedRotation}, Open: {openRotation}");
    }

    void Update()
    {
        CheckPlayerDistance();

        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !isMoving)
        {
            TryInteractWithDoor();
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
        if (isLocked)
        {
            Debug.Log(lockedPrompt);
        }
        else
        {
            string prompt = isOpen ? "Press [E] to close door" : interactionPrompt;
            Debug.Log(prompt);
        }
    }

    void HideInteractionPrompt()
    {
        // Hide interaction prompt
    }

    void TryInteractWithDoor()
    {
        if (isLocked)
        {
            // Check if player has the required key
            var itemManager = FindObjectOfType<PirateItemManager>();
            if (itemManager && itemManager.HasItem(requiredItemName))
            {
                UnlockDoor();
                OpenDoor();
            }
            else
            {
                ShowLockedMessage();
            }
        }
        else
        {
            // Door is unlocked, toggle open/close
            if (isOpen)
            {
                CloseDoor();
            }
            else
            {
                OpenDoor();
            }
        }
    }

    void UnlockDoor()
    {
        isLocked = false;

        if (audioSource && unlockSound)
        {
            audioSource.PlayOneShot(unlockSound);
        }

        Debug.Log($"Door unlocked with {requiredItemName}!");
    }

    public void OpenDoor()
    {
        if (isOpen || isMoving) return;

        StartCoroutine(RotateDoor(openRotation, true));

        if (audioSource && openSound)
        {
            audioSource.PlayOneShot(openSound);
        }

        Debug.Log("Door opening...");
    }

    public void CloseDoor()
    {
        if (!isOpen || isMoving) return;

        StartCoroutine(RotateDoor(closedRotation, false));

        if (audioSource && closeSound)
        {
            audioSource.PlayOneShot(closeSound);
        }

        Debug.Log("Door closing...");
    }

    IEnumerator RotateDoor(Vector3 targetRotation, bool opening)
    {
        isMoving = true;
        Vector3 startRotation = transform.eulerAngles;
        float elapsedTime = 0f;
        float duration = 1f / doorSpeed;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;

            // Smooth rotation using Lerp
            Vector3 currentRotation = Vector3.Lerp(startRotation, targetRotation, progress);
            transform.eulerAngles = currentRotation;

            yield return null;
        }

        // Ensure final rotation is exact
        transform.eulerAngles = targetRotation;
        isOpen = opening;
        isMoving = false;

        Debug.Log($"Door {(opening ? "opened" : "closed")}!");

        // Start auto-close timer if enabled and door just opened
        if (opening && closeAutomatically)
        {
            if (autoCloseCoroutine != null)
            {
                StopCoroutine(autoCloseCoroutine);
            }
            autoCloseCoroutine = StartCoroutine(AutoCloseTimer());
        }
    }

    IEnumerator AutoCloseTimer()
    {
        yield return new WaitForSeconds(autoCloseDelay);

        if (isOpen && !isMoving)
        {
            CloseDoor();
        }
    }

    void ShowLockedMessage()
    {
        if (audioSource && lockedSound)
        {
            audioSource.PlayOneShot(lockedSound);
        }

        Debug.Log($"The door is locked. I need to find a {requiredItemName}.");
    }

    // Public methods for external control
    public void SetLocked(bool locked)
    {
        isLocked = locked;
    }

    public void ForceOpen()
    {
        if (!isOpen)
        {
            OpenDoor();
        }
    }

    public void ForceClose()
    {
        if (isOpen)
        {
            CloseDoor();
        }
    }

    public bool IsOpen()
    {
        return isOpen;
    }

    public bool IsLocked()
    {
        return isLocked;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = isLocked ? Color.red : Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionRange);

        // Show door rotation preview in editor
        if (!Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Vector3 forward = transform.TransformDirection(Vector3.forward);
            Vector3 openDirection = Quaternion.Euler(0, openAngle, 0) * forward;

            Gizmos.DrawLine(transform.position, transform.position + forward * 2f);
            Gizmos.DrawLine(transform.position, transform.position + openDirection * 2f);
        }
    }
}

