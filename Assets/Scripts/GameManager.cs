using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI Panels")]
    public GameObject victoryPanel;
    public GameObject gameOverPanel;

    private bool gameEnded = false;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
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
        Time.timeScale = 0f; // Optional: freeze the game
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
}
