using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EscapeZone : MonoBehaviour
{
    public TextMeshProUGUI promptText;
    private bool playerInZone = false;

    void Update()
    {
        if (playerInZone && Input.GetKeyDown(KeyCode.E))
        {
            if (FindObjectOfType<ItemManager>().AllItemsCollected())
            {
                promptText.text = "You escaped successfully!";
                // TODO: 触发后续事件，例如加载胜利画面
            }
            else
            {
                promptText.text = "You haven't collected all required items.";
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = true;
            promptText.text = "Press [E] to escape";
            promptText.gameObject.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = false;
            promptText.gameObject.SetActive(false);
        }
    }
}
