using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MatchlockGun : MonoBehaviour
{
    public GameObject pickupUI; // "Press F to pick up"
    private bool playerInRange = false;

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("🔫 Gun picked up!");
            PlayerInventory.hasGun = true;
            Destroy(gameObject);

            if (pickupUI != null)
                pickupUI.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (pickupUI != null)
                pickupUI.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (pickupUI != null)
                pickupUI.SetActive(false);
        }
    }
}
