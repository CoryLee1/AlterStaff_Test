using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CaptainDialogueSystem : MonoBehaviour
{
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public ProgressiveDialogueData dialogueData;

    private string[] currentLines;
    private int index = 0;
    private bool isActive = false;

    void Update()
    {
        if (isActive && Input.GetKeyDown(KeyCode.Return)) // 支持 Enter
        {
            ShowNextLine();
        }
    }

    public void StartDialogueByProgress(int collected)
    {
        if (isActive) return;

        // 根据物品数加载相应台词段
        if (collected == 0) currentLines = dialogueData.linesWhen0Collected;
        else if (collected == 1) currentLines = dialogueData.linesWhen1Collected;
        else if (collected == 2) currentLines = dialogueData.linesWhen2Collected;
        else currentLines = dialogueData.linesWhen3Collected;

        if (currentLines == null || currentLines.Length == 0) return;

        index = 0;
        dialoguePanel.SetActive(true);
        dialogueText.text = currentLines[index];
        isActive = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

    }

    public void ShowNextLine()
    {
        index++;
        if (index < currentLines.Length)
        {
            dialogueText.text = currentLines[index];
        }
        else
        {
            EndDialogue();
        }
    }

    public void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        isActive = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

    }
}
