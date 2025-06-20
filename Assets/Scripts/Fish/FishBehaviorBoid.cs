// FishBehaviorBoid.cs
using UnityEngine;
using System.Collections.Generic;
using System;

public class FishBehaviorBoid : MonoBehaviour
{
    public float maxSpeed = 10f;
    public float neighborRadius = 5f;
    public float separationDistance = 2f;

    private Vector3 velocity;
    private static List<FishBehaviorBoid> allFish = new List<FishBehaviorBoid>();

    void Start()
    {
        velocity = UnityEngine.Random.insideUnitSphere * maxSpeed;
        allFish.Add(this);
    }

    void OnDestroy()
    {
        allFish.Remove(this);
    }

    void Update()
    {
        Vector3 alignment = Vector3.zero;
        Vector3 cohesion = Vector3.zero;
        Vector3 separation = Vector3.zero;

        int count = 0;
        foreach (var fish in allFish)
        {
            if (fish == this) continue;
            float distance = Vector3.Distance(transform.position, fish.transform.position);
            if (distance < neighborRadius)
            {
                alignment += fish.velocity;
                cohesion += fish.transform.position;
                if (distance < separationDistance)
                    separation += transform.position - fish.transform.position;
                count++;
            }
        }

        if (count > 0)
        {
            alignment /= count;
            alignment = alignment.normalized * maxSpeed;
            alignment -= velocity;

            cohesion /= count;
            cohesion = (cohesion - transform.position).normalized * maxSpeed;
            cohesion -= velocity;

            separation /= count;
            separation = separation.normalized * maxSpeed;
            separation -= velocity;
        }

        velocity += alignment + cohesion + separation;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        transform.position += velocity * Time.deltaTime;

        if (velocity != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(velocity);
    }
}
