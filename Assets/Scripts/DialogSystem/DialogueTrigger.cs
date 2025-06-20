using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public DialogueData dialogueToPlay;
    public DialogueSystem dialogueSystem;

    private bool playerInZone = false;

    void Update()
    {
        if (playerInZone && Input.GetKeyDown(KeyCode.T))
        {
            dialogueSystem.StartDialogue(dialogueToPlay);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInZone = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInZone = false;
    }
}