using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class CaptainAIController : MonoBehaviour
{
    public Transform player;
    public float chaseSpeed = 4f;
    public float gameOverDistance = 1.5f;

    private bool isDefeated = false;
    private NavMeshAgent agent;
    private Animator animator;

    public bool isAggressive = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        agent.isStopped = true;
    }

    void Update()
    {
        if (isAggressive)
        {
            if (player == null) return;

            agent.isStopped = false;
            agent.speed = chaseSpeed;
            agent.SetDestination(player.position);

            float distance = Vector3.Distance(transform.position, player.position);
            if (distance <= gameOverDistance)
            {
                TriggerGameOver();
            }
        }
    }

    public void BecomeAggressive()
    {
        isAggressive = true;
        Debug.Log("Captain has become aggressive!");

        if (animator != null)
        {
            animator.SetTrigger("Angry");
        }
    }
    public void Defeat()
    {
        if (isDefeated) return;
        isDefeated = true;

        // Stop AI movement
        if (agent != null)
            agent.isStopped = true;

        // Play death or defeat animation if available
        if (animator != null)
            animator.SetTrigger("Defeated"); // Make sure there's a trigger named "Defeated" in Animator

        // Optional: disable collider so she can't be hit again
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        Debug.Log("💀 Captain defeated!");

        // Trigger Game Win
        GameManager.Instance?.ShowVictory();  // If you have a central game manager
    }
    private void TriggerGameOver()
    {
        Debug.Log("Game Over: Caught by the captain!");
        agent.isStopped = true;

        // Example: Reload scene or open Game Over UI
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // You can replace with GameOverUI
    }
}
