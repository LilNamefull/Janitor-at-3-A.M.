using UnityEngine;

public class LookAtPickup : MonoBehaviour
{
    public float pickupRange = 3f; // Wie weit kann man das Item \"ansprechen\"?
    public LayerMask itemLayer;    // Layer für Items, z.B. \"Interactable\"

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            AttemptPickup();
        }
    }

    void AttemptPickup()
    {
        // Ray vom Kamerapunkt in Blickrichtung
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, pickupRange, itemLayer))
        {
            // Prüfen, ob Objekt ein PickupItem-Skript hat
            PickupItem pickup = hit.collider.GetComponent<PickupItem>();
            if (pickup != null)
            {
                pickup.PickUp(); // Wir rufen eine eigene Methode auf
            }
        }
    }
}
