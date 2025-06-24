using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SimpleQuestDisplay : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI questText;
    public KeyCode toggleKey = KeyCode.Tab;

    private ItemManager itemManager;
    private bool isVisible = true;

    void Start()
    {
        itemManager = FindObjectOfType<ItemManager>();

        // 每秒更新一次
        InvokeRepeating(nameof(UpdateQuestText), 0f, 1f);
    }

    void Update()
    {
        // Tab键切换显示/隐藏
        if (Input.GetKeyDown(toggleKey))
        {
            isVisible = !isVisible;
            questText.gameObject.SetActive(isVisible);
        }
    }

    void UpdateQuestText()
    {
        if (!isVisible || !questText || !itemManager) return;

        int collected = itemManager.GetCollectedCount();
        int total = itemManager.GetTotalRequiredCount();

        string questInfo = $"QUEST: Escape the Ship\n";
        questInfo += $"Items: {collected}/{total}\n";

        if (collected == 0)
        {
            questInfo += "• Explore the ship\n";
            questInfo += "• Talk to Captain (Press T)";
        }
        else if (collected < total)
        {
            questInfo += $"• Find {total - collected} more items\n";
            questInfo += "• Ask Captain for hints";
        }
        else
        {
            questInfo += "• All items found!\n";
            questInfo += "• Get key location from Captain\n";
            questInfo += "• Find key and escape!";
        }

        questInfo += $"\n\nPress [{toggleKey}] to toggle";

        questText.text = questInfo;
    }
}