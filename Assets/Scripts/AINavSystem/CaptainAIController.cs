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
    public GameObject gameOverPanel; // 拖 GameEndPanel 到这里


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
                agent.isStopped = true;  // 停止移动

                if (animator != null)
                {
                    animator.SetTrigger("Punch");  // ✅ 攻击动画 Trigger
                }

                Invoke(nameof(TriggerGameOver), 1.5f); // 延迟结束游戏（留一点攻击时间）
            }

        }
    }

    public void BecomeAggressive()
    {
        isAggressive = true;
        Debug.Log("Captain has become aggressive!");

        if (animator != null)
        {
            animator.SetTrigger("StartChase");  
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

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);  // ✅ 显示 GameEndPanel
        }
    }

}
