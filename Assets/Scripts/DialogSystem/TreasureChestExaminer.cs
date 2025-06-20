using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// =====================================================
// TreasureChestExaminer.cs - Triggers parrot hint when examining chest
// =====================================================
public class TreasureChestExaminer : MonoBehaviour
{
    [Header("Examination Settings")]
    public float examineRange = 2f;
    public string examinePrompt = "Press [E] to examine chest";
    public string lockedMessage = "The chest is locked with a combination lock...";

    private Transform player;
    private bool playerInRange = false;
    private bool hasBeenExamined = false;
    private BlueParrotDialogue parrot;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        parrot = FindObjectOfType<BlueParrotDialogue>();
    }

    void Update()
    {
        CheckPlayerDistance();

        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !hasBeenExamined)
        {
            ExamineChest();
        }
    }

    void CheckPlayerDistance()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        bool wasInRange = playerInRange;
        playerInRange = distance <= examineRange;

        if (playerInRange && !wasInRange && !hasBeenExamined)
        {
            Debug.Log(examinePrompt);
        }
    }

    void ExamineChest()
    {
        hasBeenExamined = true;

        Debug.Log(lockedMessage);

        // Trigger parrot to give hint
        if (parrot != null && !parrot.HasGivenTreasureHint())
        {
            Debug.Log("Maybe someone around here knows the combination...");
            // Parrot will give hint next time player talks to it
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, examineRange);
    }
}