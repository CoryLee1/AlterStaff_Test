using UnityEngine;
using UnityEngine.UI;

public class GunShooter : MonoBehaviour
{
    [Header("Gun Settings")]
    public float range = 100f;                    // Shooting range
    public GameObject shootVFX;                  // Optional hit visual effect

    [Header("UI")]
    public GameObject crosshairUI;               // Crosshair image to show when player holds gun

    private bool cursorUnlocked = false;

    void Update()
    {
        // Show or hide crosshair based on gun possession
        if (crosshairUI != null)
            crosshairUI.SetActive(PlayerInventory.hasGun);

        // Automatically unlock cursor once when the gun is acquired
        if (PlayerInventory.hasGun && !cursorUnlocked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            cursorUnlocked = true;
            Debug.Log("🖱 Cursor unlocked and visible for shooting.");
        }

        // Shoot on left mouse click
        if (PlayerInventory.hasGun && Input.GetMouseButtonDown(0))
        {
            FireGun();
        }
    }

    /// <summary>
    /// Raycast-based shooting logic
    /// </summary>
    private void FireGun()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            Debug.Log("🔫 Shot fired. Hit object: " + hit.collider.name);
            Debug.DrawRay(ray.origin, ray.direction * range, Color.red, 1.5f);

            if (shootVFX != null)
                Instantiate(shootVFX, hit.point, Quaternion.identity);

            // Check if we hit the captain or any of her child parts
            var captain = hit.collider.GetComponentInParent<CaptainAIController>();
            if (captain != null)
            {
                Debug.Log("💥 Captain hit through child object!");
                captain.Defeat();
            }
        }
        else
        {
            Debug.Log("🔫 Shot fired but hit nothing.");
        }
    }
}
