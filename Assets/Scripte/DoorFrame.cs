using UnityEngine;

public class DoorFrame : MonoBehaviour
{
    public Door door; // Referenz zur Tür

    void OnMouseDown()
    {
        if (door != null)
        {
            door.ToggleDoor();
        }
        else
        {
            Debug.LogError(" Keine Tür zugewiesen im Inspector!");
        }
    }
}
