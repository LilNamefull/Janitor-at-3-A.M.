using UnityEngine;

[System.Serializable]
public class Item
{
    public string itemName;      // z.B. "Schlüssel" oder "Taschenlampe"
    public Sprite icon;          // 2D-Icon für das UI (optional)
    public GameObject prefab;    // Referenz auf dein 3D-Prefab
}