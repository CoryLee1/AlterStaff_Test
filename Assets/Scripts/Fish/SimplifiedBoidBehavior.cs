using UnityEngine;

public class SimplifiedBoidBehavior : MonoBehaviour
{
    [Header("Boid Settings")]
    public float maxSpeed = 5f;
    public float neighborRadius = 3f;
    public float avoidanceRadius = 1f;

    [Header("Behavior Weights")]
    public float separationWeight = 2f;
    public float alignmentWeight = 1f;
    public float cohesionWeight = 1f;
    public float wanderWeight = 0.5f;

    private Vector3 velocity;
    private Vector3 wanderTarget;
    private float wanderAngle;
    private OptimizedFishManager fishManager;

    void Start()
    {
        velocity = Random.onUnitSphere * maxSpeed * 0.5f;
        wanderTarget = Random.onUnitSphere;
        wanderAngle = Random.Range(0f, 360f);
        fishManager = FindObjectOfType<OptimizedFishManager>();
    }

    void Update()
    {
        // 只在活跃时进行boid计算（可以进一步优化为隔帧执行）
        if (Time.frameCount % 3 == 0) // 每3帧执行一次
        {
            Vector3 boidForce = CalculateBoidForce();
            Vector3 wanderForce = CalculateWanderForce();

            Vector3 totalForce = boidForce + wanderForce * wanderWeight;
            velocity = Vector3.ClampMagnitude(velocity + totalForce * Time.deltaTime, maxSpeed);
        }

        // 移动
        transform.position += velocity * Time.deltaTime;

        // 旋转朝向
        if (velocity.sqrMagnitude > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(velocity), Time.deltaTime * 2f);
        }
    }

    Vector3 CalculateBoidForce()
    {
        if (fishManager == null) return Vector3.zero;

        var nearbyFish = fishManager.GetNearbyFish(transform.position, neighborRadius);

        Vector3 separation = Vector3.zero;
        Vector3 alignment = Vector3.zero;
        Vector3 cohesion = Vector3.zero;

        int neighborCount = 0;

        foreach (var fish in nearbyFish)
        {
            if (fish.transform == transform) continue;

            float distance = Vector3.Distance(transform.position, fish.position);

            // 分离
            if (distance < avoidanceRadius && distance > 0)
            {
                Vector3 diff = transform.position - fish.position;
                separation += diff.normalized / distance; // 距离越近，力越大
            }

            // 对齐和聚合
            if (distance < neighborRadius)
            {
                alignment += fish.velocity;
                cohesion += fish.position;
                neighborCount++;
            }
        }

        Vector3 totalForce = Vector3.zero;

        // 分离力
        if (separation.sqrMagnitude > 0)
        {
            totalForce += separation.normalized * separationWeight;
        }

        // 对齐力
        if (neighborCount > 0)
        {
            alignment /= neighborCount;
            alignment = alignment.normalized * maxSpeed;
            totalForce += (alignment - velocity) * alignmentWeight;

            // 聚合力
            cohesion /= neighborCount;
            Vector3 cohesionDirection = (cohesion - transform.position).normalized * maxSpeed;
            totalForce += (cohesionDirection - velocity) * cohesionWeight;
        }

        return totalForce;
    }

    Vector3 CalculateWanderForce()
    {
        // 简单的漫游行为
        wanderAngle += Random.Range(-30f, 30f) * Time.deltaTime;

        Vector3 circleCenter = transform.position + velocity.normalized * 2f;
        Vector3 displacement = new Vector3(Mathf.Cos(wanderAngle), 0, Mathf.Sin(wanderAngle));
        wanderTarget = circleCenter + displacement;

        return (wanderTarget - transform.position).normalized;
    }
}