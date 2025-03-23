using UnityEngine;

public class PickupItem : MonoBehaviour
{
    public Item itemData; // Referenz auf das Item-Objekt (Name, Icon, Prefab)

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Füge das Item ins Inventar
            Inventory inv = FindObjectOfType<Inventory>();
            if (inv.AddItem(itemData))
            {
                // Zerstöre das 3D-Objekt in der Szene
                Destroy(gameObject);
            }
        }
    }
}

