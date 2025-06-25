using UnityEngine;

public class MouseLockController : MonoBehaviour
{
    public static MouseLockController Instance;

    private int uiRequestCount = 0; // 同时有多个 UI 开时不冲突

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
        LockCursor(); // 初始状态锁定
    }

    public void RequestCursor(bool unlock)
    {
        if (unlock)
        {
            uiRequestCount++;
            UnlockCursor();
        }
        else
        {
            uiRequestCount = Mathf.Max(0, uiRequestCount - 1);
            if (uiRequestCount == 0)
                LockCursor();
        }
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
