using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "NewProgressiveDialogue", menuName = "Dialogue/Progressive Dialogue Data")]
public class ProgressiveDialogueData : ScriptableObject
{
    [Header("Normal Progression Dialogues")]
    [TextArea(2, 5)]
    public string[] linesWhen0Collected = {
        "Ah, a new face aboard my ship!",
        "You look like you're lost, sailor.",
        "This is MY ship, and everything on it belongs to ME!",
        "Don't even think about touching anything!"
    };

    [TextArea(2, 5)]
    public string[] linesWhen1Collected = {
        "Hey! I see you've been snooping around!",
        "You found one of my precious items, didn't you?",
        "Well, no matter. You'll never find them all!",
        "My treasures are well hidden across this ship!"
    };

    [TextArea(2, 5)]
    public string[] linesWhen2Collected = {
        "What?! You found TWO of my items?",
        "You're more clever than I thought...",
        "But don't get too confident, landlubber!",
        "The most important things are still safely hidden!"
    };

    [TextArea(2, 5)]
    public string[] linesWhen3Collected = {
        "Impossible! You found ALL my precious items!",
        "You think you're so smart, don't you?",
        "Well, even if you have those trinkets...",
        "You'll NEVER escape this ship alive!"
    };

    [Header("Lifeboat Key Location Hints")]
    [TextArea(3, 5)]
    public string[] keyLocationHintTemplate = {
        "Ha! You want to escape, do you?",
        "Well, you'll need the lifeboat key for that!",
        "And I've hidden it somewhere very special...",
        "It's in {keyLocation}, where you'll never think to look!",
        "Good luck finding it before I throw you overboard!"
    };

    [Header("Alternative Key Hint (if needed)")]
    [TextArea(3, 5)]
    public string[] alternativeKeyHint = {
        "Looking for a way off my ship?",
        "The lifeboat won't do you any good without its key!",
        "I've placed it {keyLocation}...",
        "You'd have to be very lucky to stumble upon it!"
    };

    // 方法：获取处理后的Key位置对话
    public string[] GetKeyLocationDialogue(string keyLocation)
    {
        if (string.IsNullOrEmpty(keyLocation))
            keyLocation = "somewhere you'll never find";

        return keyLocationHintTemplate.Select(line =>
            line.Replace("{keyLocation}", keyLocation)).ToArray();
    }

    // 方法：获取备用Key位置对话
    public string[] GetAlternativeKeyDialogue(string keyLocation)
    {
        if (string.IsNullOrEmpty(keyLocation))
            keyLocation = "in a place beyond your reach";

        return alternativeKeyHint.Select(line =>
            line.Replace("{keyLocation}", keyLocation)).ToArray();
    }

    // 方法：根据收集进度获取标准对话
    public string[] GetProgressDialogue(int collectedCount)
    {
        switch (collectedCount)
        {
            case 0: return linesWhen0Collected;
            case 1: return linesWhen1Collected;
            case 2: return linesWhen2Collected;
            case 3: return linesWhen3Collected;
            default: return linesWhen3Collected;
        }
    }
}