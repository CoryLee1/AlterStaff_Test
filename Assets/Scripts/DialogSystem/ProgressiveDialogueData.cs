using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewProgressiveDialogue", menuName = "Dialogue/Progressive Dialogue Data")]
public class ProgressiveDialogueData : ScriptableObject
{
    [TextArea(2, 5)]
    public string[] linesWhen0Collected;

    [TextArea(2, 5)]
    public string[] linesWhen1Collected;

    [TextArea(2, 5)]
    public string[] linesWhen2Collected;

    [TextArea(2, 5)]
    public string[] linesWhen3Collected;
}
