using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class InventoryItem : ScriptableObject
{
    public string itemName;
    public Sprite icon;

    public virtual void Use()
    {
        Debug.Log("Benutze " + itemName);
    }
}
