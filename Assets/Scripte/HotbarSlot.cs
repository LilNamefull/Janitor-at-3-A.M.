using UnityEngine;
using UnityEngine.UI;

public class HotbarSlot : MonoBehaviour
{
    public Image icon;
    private InventoryItem currentItem;

    public void SetItem(InventoryItem item)
    {
        currentItem = item;
        icon.sprite = item.icon;
        icon.enabled = true;
    }

    public void ClearSlot()
    {
        currentItem = null;
        icon.sprite = null;
        icon.enabled = false;
    }

    public bool HasItem()
    {
        return currentItem != null;
    }

    public void UseItem()
    {
        if (currentItem != null)
        {
            currentItem.Use();
        }
    }

    public void SetHighlight(bool isSelected)
    {
        GetComponent<Image>().color = isSelected ? Color.yellow : Color.white;
    }
}
