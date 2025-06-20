using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class ShipWheelInteract : MonoBehaviour
{
    public GameObject promptUI; // 显示 Press T to take control
    public CaptainAIController captainAI; // 引用船长AI

    private bool playerInZone = false;
    private bool hasInteracted = false;

    void Update()
    {
        if (playerInZone && !hasInteracted && Input.GetKeyDown(KeyCode.T))
        {
            hasInteracted = true;

            if (promptUI != null)
                promptUI.SetActive(false);

            Debug.Log("🚨 Player tried to take control of the ship!");

            if (captainAI != null)
                captainAI.BecomeAggressive();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = true;
            if (!hasInteracted && promptUI != null)
                promptUI.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = false;
            if (promptUI != null)
                promptUI.SetActive(false);
        }
    }
}
