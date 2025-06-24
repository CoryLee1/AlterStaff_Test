using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI Panels")]
    public GameObject victoryPanel;
    public GameObject gameOverPanel;

    [Header("Lifeboat System")]
    public GameObject lifeboat;                    // 救生艇GameObject
    public GameObject lifeboatInteractionUI;       // 救生艇交互UI
    public AudioClip lifeboatActivationSound;      // 激活救生艇的音效

    [Header("Collection Requirements")]
    public bool requireAllItems = true;            // 是否需要收集所有物品才能激活救生艇
    public List<string> essentialItems;            // 必需的关键物品（如果不需要全部收集）

    private bool gameEnded = false;
    private bool lifeboatActivated = false;
    private ItemManager itemManager;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        InitializeGame();
    }

    void Update()
    {
        // 持续检查收集状态
        if (!lifeboatActivated && !gameEnded)
        {
            CheckCollectionProgress();
        }
    }

    /// <summary>
    /// 初始化游戏
    /// </summary>
    public void InitializeGame()
    {
        // 获取ItemManager引用
        itemManager = FindObjectOfType<ItemManager>();
        if (itemManager == null)
        {
            Debug.LogError("GameManager: No ItemManager found!");
            return;
        }

        // 确保救生艇初始状态是关闭的
        SetLifeboatState(false);
        lifeboatActivated = false;

        Debug.Log("🎮 Game initialized. Collect all items to activate the lifeboat!");
        Debug.Log($"📋 Current objective: {GetCollectionProgress()}");
    }

    /// <summary>
    /// 检查收集进度并决定是否激活救生艇
    /// </summary>
    private void CheckCollectionProgress()
    {
        if (itemManager == null) return;

        bool canActivateLifeboat = false;

        if (requireAllItems)
        {
            // 需要收集所有物品
            canActivateLifeboat = itemManager.AllItemsCollected();
        }
        else
        {
            // 只需要收集关键物品
            canActivateLifeboat = CheckEssentialItemsCollected();
        }

        if (canActivateLifeboat && !lifeboatActivated)
        {
            ActivateLifeboat();
        }
    }

    /// <summary>
    /// 检查关键物品是否都已收集
    /// </summary>
    private bool CheckEssentialItemsCollected()
    {
        if (essentialItems == null || essentialItems.Count == 0)
        {
            return itemManager.AllItemsCollected(); // 如果没有指定关键物品，则需要全部收集
        }

        foreach (string itemName in essentialItems)
        {
            // 这里需要从ItemManager获取特定物品的收集状态
            // 我们需要在ItemManager中添加一个方法来检查特定物品
            if (!itemManager.IsItemCollected(itemName))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 激活救生艇
    /// </summary>
    private void ActivateLifeboat()
    {
        lifeboatActivated = true;
        SetLifeboatState(true);

        // 播放激活音效
        if (lifeboatActivationSound != null)
        {
            AudioSource.PlayClipAtPoint(lifeboatActivationSound, transform.position);
        }

        // 显示救生艇激活消息
        Debug.Log("🚤 Lifeboat activated! You can now escape!");

        // 可以添加UI提示
        if (lifeboatInteractionUI != null)
        {
            lifeboatInteractionUI.SetActive(true);
            StartCoroutine(HideLifeboatUIAfterDelay(5f)); // 5秒后隐藏提示
        }

        // 触发救生艇激活事件（可以通知其他系统）
        OnLifeboatActivated();
    }

    /// <summary>
    /// 设置救生艇状态
    /// </summary>
    private void SetLifeboatState(bool isActive)
    {
        if (lifeboat != null)
        {
            // 可以控制救生艇的可交互性
            var lifeboatCollider = lifeboat.GetComponent<Collider>();
            if (lifeboatCollider != null)
            {
                lifeboatCollider.enabled = isActive;
            }

            // 可以改变救生艇的视觉效果
            var lifeboatRenderer = lifeboat.GetComponent<Renderer>();
            if (lifeboatRenderer != null)
            {
                lifeboatRenderer.material.color = isActive ? Color.green : Color.gray;
            }

            // 启用/禁用救生艇脚本
            var lifeboatScript = lifeboat.GetComponent<MonoBehaviour>();
            if (lifeboatScript != null)
            {
                lifeboatScript.enabled = isActive;
            }
        }
    }

    /// <summary>
    /// 救生艇激活后的事件处理
    /// </summary>
    private void OnLifeboatActivated()
    {
        // 这里可以触发其他系统的响应
        // 比如：改变音乐、显示新的UI、启用新的游戏机制等

        // 示例：更新游戏目标
        UpdateGameObjective("Escape using the lifeboat!");
    }

    /// <summary>
    /// 更新游戏目标显示
    /// </summary>
    private void UpdateGameObjective(string newObjective)
    {
        // 这里可以更新UI显示当前游戏目标
        Debug.Log($"📋 New Objective: {newObjective}");

        // 如果有目标UI，在这里更新
        // objectiveText.text = newObjective;
    }

    /// <summary>
    /// 延迟隐藏救生艇UI提示
    /// </summary>
    private IEnumerator HideLifeboatUIAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (lifeboatInteractionUI != null)
        {
            lifeboatInteractionUI.SetActive(false);
        }
    }

    /// <summary>
    /// 玩家使用救生艇逃脱（由救生艇脚本调用）
    /// </summary>
    public void EscapeWithLifeboat()
    {
        if (!lifeboatActivated)
        {
            Debug.Log("❌ Lifeboat is not activated yet! Collect all required items first.");
            return;
        }

        Debug.Log("🏆 Player escaped with the lifeboat!");
        ShowVictory();
    }

    /// <summary>
    /// 获取当前收集进度（供UI显示）
    /// </summary>
    public string GetCollectionProgress()
    {
        if (itemManager == null) return "ItemManager not found";

        int collectedCount = itemManager.GetCollectedCount();
        int totalRequired = 0;

        // 计算总需求数量
        if (requireAllItems)
        {
            // 需要计算所有物品的总需求
            totalRequired = itemManager.GetTotalRequiredCount();
        }
        else
        {
            totalRequired = essentialItems?.Count ?? 0;
        }

        return $"Progress: {collectedCount}/{totalRequired}";
    }

    /// <summary>
    /// 检查救生艇是否已激活
    /// </summary>
    public bool IsLifeboatActivated()
    {
        return lifeboatActivated;
    }

    /// <summary>
    /// Call this when the player successfully defeats the captain.
    /// </summary>
    public void ShowVictory()
    {
        if (gameEnded) return;
        gameEnded = true;

        if (victoryPanel != null)
            victoryPanel.SetActive(true);

        Debug.Log("🏁 Victory triggered");
        Time.timeScale = 0f;
    }

    /// <summary>
    /// Call this when the player fails or gets caught.
    /// </summary>
    public void ShowGameOver()
    {
        if (gameEnded) return;
        gameEnded = true;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Debug.Log("💀 Game Over triggered");
        Time.timeScale = 0f;
    }

    /// <summary>
    /// Resume game time if needed (e.g., retry)
    /// </summary>
    public void ResumeGame()
    {
        Time.timeScale = 1f;
        gameEnded = false;
    }

    /// <summary>
    /// 重新开始游戏
    /// </summary>
    public void RestartGame()
    {
        ResumeGame();
        lifeboatActivated = false;

        // 重新初始化ItemManager
        if (itemManager != null)
        {
            itemManager.ClearAndRespawnItems();
        }

        // 重新设置救生艇状态
        SetLifeboatState(false);

        // 隐藏UI面板
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (lifeboatInteractionUI != null) lifeboatInteractionUI.SetActive(false);

        Debug.Log("🔄 Game restarted!");
    }
}