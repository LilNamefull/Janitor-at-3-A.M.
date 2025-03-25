using UnityEngine;

public class PickupKey : MonoBehaviour
{
    private Hotbar hotbar; // Referenz zum Hotbar-Skript

    void Start()
    {
        // Holen des ersten Hotbar-Skripts in der Szene
        hotbar = Object.FindFirstObjectByType<Hotbar>();

        if (hotbar == null)
        {
            Debug.LogError("Kein Hotbar-Skript gefunden. Stelle sicher, dass ein HotbarManager in der Szene existiert.");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Wenn der Trigger das Tag "Player" hat und die Referenz zum Hotbar-Skript vorhanden ist
        if (other.CompareTag("Player") && hotbar != null)
        {
            hotbar.PickupKey(); // Schlüssel aufheben
            Destroy(gameObject); // Entferne den Schlüssel aus der Szene
        }
    }
}
