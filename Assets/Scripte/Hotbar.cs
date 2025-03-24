using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hotbar : MonoBehaviour
{
    public int hotbarSize = 5; // Anzahl der Hotbar-Slots
    public Transform hotbarParent; // Parent für die Slots
    public GameObject slotPrefab; // Prefab für die Hotbar-Slots

    private List<HotbarSlot> slots = new List<HotbarSlot>();
    private int selectedSlotIndex = 0; // Welcher Slot aktuell aktiv ist

    void Start()
    {
        // Hotbar-Slots erstellen
        for (int i = 0; i < hotbarSize; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, hotbarParent);
            HotbarSlot slot = slotObj.GetComponent<HotbarSlot>();
            slots.Add(slot);
        }

        HighlightSelectedSlot();
    }

    void Update()
    {
        HandleScrollInput();
    }

    private void HandleScrollInput()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
        {
            selectedSlotIndex = (selectedSlotIndex + 1) % hotbarSize;
        }
        else if (scroll < 0f)
        {
            selectedSlotIndex = (selectedSlotIndex - 1 + hotbarSize) % hotbarSize;
        }

        HighlightSelectedSlot();
    }

    private void HighlightSelectedSlot()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].SetHighlight(i == selectedSlotIndex);
        }
    }

    public void UseSelectedItem()
    {
        if (slots[selectedSlotIndex].HasItem())
        {
            slots[selectedSlotIndex].UseItem();
        }
    }
}

