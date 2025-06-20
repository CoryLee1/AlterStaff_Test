using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MatchlockGunSpawner : MonoBehaviour
{
    public GameObject gunPrefab;
    public Transform spawnPoint;
    public CaptainAIController captainAI;

    private bool spawned = false;

    void Update()
    {
        if (!spawned && captainAI != null && captainAI.isAggressive)
        {
            Instantiate(gunPrefab, spawnPoint.position, Quaternion.identity);
            spawned = true;
        }
    }
}
