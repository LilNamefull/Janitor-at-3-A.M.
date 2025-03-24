using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public GameObject inventoryPanel;  // Das UI-Panel für das Inventar
    public Transform slotParent;       // Parent für die Slots
    public GameObject slotPrefab;      // Prefab für einen einzelnen Slot

    [Header("Referenz auf deinen Player-/Kamera-Controller")]
    public MonoBehaviour playerController; // Hier ziehst du dein FPS-/Movement-Skript rein

    private Inventory inventory;
    private List<InventorySlot> slots = new List<InventorySlot>();
    private bool isOpen = false;

    void Start()
    {
        inventory = FindAnyObjectByType<Inventory>();
        if (inventory == null)
        {
            Debug.LogError("Kein Inventory in Szene gefunden!");
            return;
        }

        // Erstelle so viele Slots wie maxSlots
        int maxSlots = inventory.maxSlots;
        for (int i = 0; i < maxSlots; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, slotParent);
            InventorySlot slot = slotObj.GetComponent<InventorySlot>();
            slot.ClearSlot();
            slots.Add(slot);
        }

        inventoryPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        isOpen = !isOpen;
        inventoryPanel.SetActive(isOpen);

        if (isOpen) UpdateInventoryUI();
        if (isOpen)
        {
            // INVENTAR ÖFFNET SICH:
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Spielerbewegung & Kamera ausschalten
            if (playerController != null)
                playerController.enabled = false;

            // (Optional) Spiel pausieren:
            // Time.timeScale = 0f;
        }
        else
        {
            // INVENTAR SCHLIESST SICH:
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Spielerbewegung & Kamera wieder an
            if (playerController != null)
                playerController.enabled = true;

            // (Optional) Spiel fortsetzen:
            // Time.timeScale = 1f;
        }
    }

    public void UpdateInventoryUI()
    {
        List<InventoryItem> currentItems = inventory.items;

        for (int i = 0; i < slots.Count; i++)
        {
            if (i < currentItems.Count)
            {
                slots[i].SetItem(currentItems[i]);
            }
            else
            {
                slots[i].ClearSlot();
            }
        }
    }
}
