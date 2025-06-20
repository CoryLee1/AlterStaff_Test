using UnityEngine;
using Cinemachine;
using System.Linq;

public class FishCamera : MonoBehaviour
{
    [Header("Camera Settings")]
    public Camera mainCamera;
    public Camera fishCamera;
    public KeyCode switchKey = KeyCode.C;
    public float followDistance = 5f;
    public float followHeight = 2f;
    public float smoothTime = 0.3f;

    private bool isFollowingFish = false;
    private Transform currentFishTarget;
    private OptimizedFishManager fishManager;
    private Vector3 velocity;

    void Start()
    {
        fishManager = FindObjectOfType<OptimizedFishManager>();
        fishCamera.gameObject.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(switchKey))
        {
            ToggleFishCamera();
        }

        if (isFollowingFish && currentFishTarget != null)
        {
            UpdateFishCamera();
        }
    }

    void ToggleFishCamera()
    {
        isFollowingFish = !isFollowingFish;

        if (isFollowingFish)
        {
            // 随机选择一条鱼跟随
            var activeFish = fishManager.activeFish;
            if (activeFish.Count > 0)
            {
                var randomFish = activeFish[Random.Range(0, activeFish.Count)];
                currentFishTarget = randomFish.transform;

                mainCamera.gameObject.SetActive(false);
                fishCamera.gameObject.SetActive(true);
            }
        }
        else
        {
            mainCamera.gameObject.SetActive(true);
            fishCamera.gameObject.SetActive(false);
            currentFishTarget = null;
        }
    }

    void UpdateFishCamera()
    {
        if (currentFishTarget == null) return;

        // 计算目标位置（鱼的后方上方）
        Vector3 targetPosition = currentFishTarget.position
            - currentFishTarget.forward * followDistance
            + Vector3.up * followHeight;

        // 平滑移动
        fishCamera.transform.position = Vector3.SmoothDamp(
            fishCamera.transform.position,
            targetPosition,
            ref velocity,
            smoothTime);

        // 看向鱼
        fishCamera.transform.LookAt(currentFishTarget);
    }
}