using UnityEngine;
using UnityEngine.AI;
using TMPro;

public class CaptainTrigger : MonoBehaviour
{
    public Transform targetPoint;
    public TextMeshProUGUI promptText;

    private bool playerInZone = false;
    private bool hasInteracted = false;
    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (promptText != null)
            promptText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (playerInZone && !hasInteracted && Input.GetKeyDown(KeyCode.T))
        {
            hasInteracted = true;
            if (promptText != null)
                promptText.gameObject.SetActive(false);

            agent.SetDestination(targetPoint.position);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasInteracted)
        {
            playerInZone = true;
            if (promptText != null)
                promptText.gameObject.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = false;
            if (promptText != null)
                promptText.gameObject.SetActive(false);
        }
    }
}
