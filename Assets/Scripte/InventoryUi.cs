using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("Inventory Panel")]
    public GameObject inventoryPanel; // Das UI-Panel für das Inventar

    [Header("Player Control Script")]
    public MonoBehaviour playerController;
    //  Hier trägst du später im Inspector dein FPS-/Camera-Skript ein, 
    //   das du deaktivieren möchtest, wenn das Inventar offen ist.

    private bool isOpen = false;

    void Update()
    {
        // Inventar ein-/ausblenden mit Taste I
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }
    }

    private void ToggleInventory()
    {
        isOpen = !isOpen;
        inventoryPanel.SetActive(isOpen);

        if (isOpen)
        {
            // INVENTAR IST OFFEN:
            // 1. Mauszeiger freigeben
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // 2. FPS-Steuerung (Camera/Movement) deaktivieren
            if (playerController != null)
                playerController.enabled = false;
        }
        else
        {
            // INVENTAR IST ZU:
            // 1. Mauszeiger sperren
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // 2. FPS-Steuerung wieder aktivieren
            if (playerController != null)
                playerController.enabled = true;
        }
    }
}

