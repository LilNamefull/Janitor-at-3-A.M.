using UnityEngine;

public class HotbarManager : MonoBehaviour
{
    public HotbarUI hotbarUI;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) UseHotbarItem(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) UseHotbarItem(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) UseHotbarItem(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) UseHotbarItem(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) UseHotbarItem(4);
    }

    void UseHotbarItem(int index)
    {
        InventorySlot slot = hotbarUI.GetSelectedSlot();
        if (slot != null && slot.HasItem())
        {
            slot.UseItem();
        }
    }
}
