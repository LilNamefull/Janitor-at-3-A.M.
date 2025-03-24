using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public GameObject inventoryPanel;
    public Transform slotParent;
    public GameObject slotPrefab;

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
