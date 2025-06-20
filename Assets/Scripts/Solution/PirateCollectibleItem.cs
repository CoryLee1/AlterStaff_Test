// =====================================================
// PirateCollectibleItem.cs - Collectible item behavior for pirate ship
// =====================================================

using UnityEngine;

public class PirateCollectibleItem : MonoBehaviour
{
    [Header("Item Settings")]
    public PirateItemData itemData;

    [Header("Interaction Settings")]
    public float interactionRange = 3f;
    public KeyCode pickupKey = KeyCode.E;

    [Header("Visual Effects")]
    public GameObject highlightEffect;
    public ParticleSystem pickupEffect;

    private bool canPickup = false;
    private bool isPickedUp = false;
    private Transform player;
    private AudioSource audioSource;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        audioSource = GetComponent<AudioSource>();

        if (highlightEffect) highlightEffect.SetActive(false);
    }

    void Update()
    {
        if (isPickedUp) return;

        CheckPlayerDistance();

        if (canPickup && Input.GetKeyDown(pickupKey))
        {
            PickupItem();
        }
    }

    void CheckPlayerDistance()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        bool wasCanPickup = canPickup;
        canPickup = distance <= interactionRange;

        if (canPickup && !wasCanPickup)
        {
            OnPlayerEnterRange();
        }
        else if (!canPickup && wasCanPickup)
        {
            OnPlayerExitRange();
        }
    }

    void OnPlayerEnterRange()
    {
        if (highlightEffect) highlightEffect.SetActive(true);

        string prompt = $"Press [{pickupKey}] to collect {itemData.displayName}";
        PirateItemManager.Instance?.ShowInteractionPrompt(prompt);
    }

    void OnPlayerExitRange()
    {
        if (highlightEffect) highlightEffect.SetActive(false);
        PirateItemManager.Instance?.HideInteractionPrompt();
    }

    void PickupItem()
    {
        if (isPickedUp) return;

        isPickedUp = true;

        // Report to manager
        PirateItemManager.Instance?.ReportCollected(itemData.itemName);

        // Play sound effect
        if (audioSource && itemData.pickupSound)
        {
            audioSource.PlayOneShot(itemData.pickupSound);
        }

        // Play visual effect
        if (pickupEffect)
        {
            pickupEffect.Play();
            // Delay destruction to wait for effect completion
            Destroy(gameObject, pickupEffect.main.duration);
        }
        else
        {
            Destroy(gameObject);
        }

        // Hide interaction prompt
        PirateItemManager.Instance?.HideInteractionPrompt();

        Debug.Log($"Picked up: {itemData.displayName}");
    }

    public void SetItemData(PirateItemData data)
    {
        itemData = data;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}