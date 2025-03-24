using UnityEngine;

public class LookAtPickup : MonoBehaviour
{
    public float pickupRange = 3f;
    public LayerMask itemLayer;
    public Camera playerCamera;


    void Start()
    {
        // Fallback, falls Kamera nicht im Inspector zugewiesen wurde
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            Debug.LogWarning("PlayerCamera nicht im Inspector zugewiesen, using Camera.main", this);
        }
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            AttemptPickup();
        }
    }

    void AttemptPickup()
    {
        if (playerCamera == null)
        {
            Debug.LogError("Keine Kamera-Referenz vorhanden!", this);
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * pickupRange, Color.red, 1f); // Visualisierung

        if (Physics.Raycast(ray, out hit, pickupRange, itemLayer))
        {
            PickupItem pickup = hit.collider.GetComponent<PickupItem>();
            if (pickup != null)
            {
                pickup.PickUp();
            }
        }
    }
}