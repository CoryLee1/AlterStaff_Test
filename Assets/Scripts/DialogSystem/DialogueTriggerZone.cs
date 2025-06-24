using UnityEngine;
using TMPro;

public class DialogueTriggerZone : MonoBehaviour
{
    public CaptainDialogueSystem dialogueSystem;
    public TextMeshProUGUI talkPromptText;

    private bool playerInZone = false;

    void Update()
    {
        if (playerInZone && Input.GetKeyDown(KeyCode.T))
        {
            if (dialogueSystem.IsActive)
            {
                Debug.Log("Dialogue already active, ignoring T key press");
                return;
            }

            int collected = FindObjectOfType<ItemManager>().GetCollectedCount();
            dialogueSystem.StartDialogueByProgress(collected);

            Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
            CaptainAIController captain = FindObjectOfType<CaptainAIController>();

            if (player != null && captain != null)
            {
                Vector3 directionToPlayer = player.position - captain.transform.position;
                directionToPlayer.y = 0;
                if (directionToPlayer != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                    captain.transform.rotation = targetRotation;
                }

                Animator animator = captain.GetComponent<Animator>();
                if (animator != null)
                    animator.SetTrigger("StartTalking");
            }

            if (talkPromptText != null)
                talkPromptText.gameObject.SetActive(false);
        }

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = true;
            Debug.Log("Player entered dialogue zone"); // 调试信息

            if (talkPromptText != null && !dialogueSystem.IsActive) // 使用公共属性
                talkPromptText.gameObject.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = false;
            Debug.Log("Player left dialogue zone"); // 调试信息

            if (talkPromptText != null)
                talkPromptText.gameObject.SetActive(false);
        }
    }

    // 调试方法
    void OnGUI()
    {
        if (playerInZone)
        {
            GUI.Label(new Rect(10, 10, 200, 20), "Player in zone: TRUE");
            GUI.Label(new Rect(10, 30, 200, 20), $"Dialogue active: {dialogueSystem.IsActive}"); // 使用公共属性
        }
    }
}