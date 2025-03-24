using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HotbarUI : MonoBehaviour
{
    public Transform slotParent;
    public GameObject slotPrefab;
    public int slotCount = 5;
    private List<InventorySlot> slots = new List<InventorySlot>();
    private int selectedSlotIndex = 0;

    void Start()
    {
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
                slotImage.color = Color.yellow;
            else
                slotImage.color = Color.white;
        }
    }

    public InventorySlot GetSelectedSlot()
    {
        return slots[selectedSlotIndex];
    }
}
