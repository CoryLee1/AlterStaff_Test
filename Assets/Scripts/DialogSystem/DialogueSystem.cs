using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueSystem : MonoBehaviour
{
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;

    private string[] lines;
    private int index = 0;
    private bool isActive = false;

    void Update()
    {
        if (!isActive) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            index++;
            if (index < lines.Length)
            {
                dialogueText.text = lines[index];
            }
            else
            {
                EndDialogue();
            }
        }
    }

    public void StartDialogue(DialogueData dialogue)
    {
        lines = dialogue.lines;
        index = 0;
        dialoguePanel.SetActive(true);
        dialogueText.text = lines[index];
        isActive = true;
    }

    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        isActive = false;
    }
}
