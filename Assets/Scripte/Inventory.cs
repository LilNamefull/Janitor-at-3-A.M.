using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public int maxSlots = 20;
    public List<InventoryItem> items = new List<InventoryItem>();

    public bool AddItem(InventoryItem newItem)
    {
        if (items.Count >= maxSlots)
        {
            Debug.Log("Inventar ist voll!");
            return false;
        }
        items.Add(newItem);
        Debug.Log("Item hinzugefügt: " + newItem.itemName);
        return true;
    }

    public void RemoveItem(InventoryItem item)
    {
        if (items.Contains(item))
        {
            items.Remove(item);
            Debug.Log("Item entfernt: " + item.itemName);
        }
    }
}
