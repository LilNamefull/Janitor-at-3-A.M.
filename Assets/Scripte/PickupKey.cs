
using UnityEngine;

public class PickupKey : MonoBehaviour
{
    private bool collected = false;

    private void OnTriggerEnter(Collider other)
    {
        if (collected) return;

        if (other.CompareTag("Player"))
        {
            collected = true;

            // Schlüssel im GameManager zählen
            GameManager.Instance.CollectKey();

            // Schlüssel entfernen
            Destroy(gameObject);
        }
    }
}
