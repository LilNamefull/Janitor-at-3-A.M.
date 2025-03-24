using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HotbarUI : MonoBehaviour
{
    public Transform slotParent; // Parent für die Slots
    public GameObject slotPrefab; // Prefab für einzelne Slots
    public int slotCount = 5; // Anzahl der Slots in der Hotbar
    private List<InventorySlot> slots = new List<InventorySlot>();
    private int selectedSlotIndex = 0;

    void Start()
    {
        // Hotbar-Slots erstellen
        for (int i = 0; i < slotCount; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, slotParent);
            InventorySlot slot = slotObj.GetComponent<InventorySlot>();
            slots.Add(slot);
        }

        HighlightSelectedSlot();
    }

    void Update()
    {
        ScrollHotbar();
    }

    void ScrollHotbar()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
        {
            selectedSlotIndex = (selectedSlotIndex + 1) % slotCount;
        }
        else if (scroll < 0f)
        {
            selectedSlotIndex = (selectedSlotIndex - 1 + slotCount) % slotCount;
        }

        HighlightSelectedSlot();
    }

    void HighlightSelectedSlot()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            Image slotImage = slots[i].GetComponent<Image>();
            if (i == selectedSlotIndex)
                slotImage.color = Color.yellow; // Markiere ausgewählten Slot
            else
                slotImage.color = Color.white;
        }
    }

    public InventorySlot GetSelectedSlot()
    {
        return slots[selectedSlotIndex];
    }
}
