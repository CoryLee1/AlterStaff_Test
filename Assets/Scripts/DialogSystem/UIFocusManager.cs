using UnityEngine;

public class UIFocusManager : MonoBehaviour
{
    public static UIFocusManager Instance;
    public bool uiIsActive = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (uiIsActive)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void EnterUIMode()
    {
        uiIsActive = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ExitUIMode()
    {
        uiIsActive = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
