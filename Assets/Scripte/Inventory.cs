using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public int maxSlots = 20;
    public List<Item> items = new List<Item>(); // Die eigentliche Item-Liste

    // Füge ein Item hinzu, wenn Platz ist
    public bool AddItem(Item newItem)
    {
        if (items.Count >= maxSlots)
        {
            Debug.Log("Inventar ist voll!");
            return false;
        }

        items.Add(newItem);
        Debug.Log("Item hinzugefügt: " + newItem.itemName);
        // Hier könntest du UI aktualisieren:
        // InventoryUI.Instance.UpdateUI();
        return true;
    }

    // Entferne ein Item
    public void RemoveItem(Item itemToRemove)
    {
        if (items.Contains(itemToRemove))
        {
            items.Remove(itemToRemove);
            Debug.Log("Item entfernt: " + itemToRemove.itemName);
            // UI aktualisieren:
            // InventoryUI.Instance.UpdateUI();
        }
    }

    // Aktuelle Liste aller Items
    public List<Item> GetItemList()
    {
        return items;
    }
}

