using UnityEngine;

public class PickupItem : MonoBehaviour
{
    public InventoryItem itemData;
    public bool destroyOnPickup = true;

    public void PickUp()
    {
        Inventory inv = FindAnyObjectByType<Inventory>();
        if (inv != null)
        {
            bool added = inv.AddItem(itemData);
            if (added && destroyOnPickup)
            {
                Destroy(gameObject);
            }
        }
    }
}

