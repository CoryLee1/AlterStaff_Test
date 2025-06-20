using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CollectibleItem : MonoBehaviour
{
    public string itemName;  // 如 "key", "gear"
    public TextMeshProUGUI promptText;  // 拖入场景中的提示文本

    private bool canPickup = false;

    void Update()
    {
        if (canPickup && Input.GetKeyDown(KeyCode.P))
        {
            FindObjectOfType<ItemManager>().ReportCollected(itemName);
            if (promptText != null)
                promptText.gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canPickup = true;
            if (promptText != null)
            {
                promptText.text = $"Press [P] to collect {itemName}";
                promptText.gameObject.SetActive(true);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canPickup = false;
            if (promptText != null)
                promptText.gameObject.SetActive(false);
        }
    }
}
