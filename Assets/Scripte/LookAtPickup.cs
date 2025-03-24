using UnityEngine;

public class LookAtPickup : MonoBehaviour
{
    public float pickupRange = 3f;
    public LayerMask itemLayer;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            AttemptPickup();
        }
    }

    void AttemptPickup()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit hit;

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