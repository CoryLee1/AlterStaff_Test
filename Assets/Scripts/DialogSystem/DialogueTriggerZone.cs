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
            int collected = FindObjectOfType<ItemManager>().GetCollectedCount();
            dialogueSystem.StartDialogueByProgress(collected);

            if (talkPromptText != null)
                talkPromptText.gameObject.SetActive(false); // 对话期间隐藏提示
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = true;
            if (talkPromptText != null)
                talkPromptText.gameObject.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = false;
            if (talkPromptText != null)
                talkPromptText.gameObject.SetActive(false);
        }
    }


}
