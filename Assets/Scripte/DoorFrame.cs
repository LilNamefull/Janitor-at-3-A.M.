using UnityEngine;

public class DoorFrame : MonoBehaviour
{
    public Door door; // Referenz zur Tür
    public Transform player; // Referenz zum Spieler
    public float interactionDistance = 3f; // Maximale Distanz für Interaktion
    public LayerMask interactableLayer; // Nur bestimmte Objekte als Türrahmen erkennen

    void Start()
    {
        if (player == null) // Falls der Spieler nicht manuell zugewiesen wurde, suche ihn
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && IsPlayerLookingAtFrame())
        {
            door.Interact(); // Tür öffnen oder schließen
        }
    }

    private bool IsPlayerLookingAtFrame()
    {
        if (player == null) return false;

        // Prüfe die Entfernung
        float distance = Vector3.Distance(player.position, transform.position);
        if (distance > interactionDistance) return false;

        // Prüfe, ob der Spieler auf den Türrahmen schaut (Raycast)
        Ray ray = new Ray(player.position, player.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
        {
            return hit.transform == transform; // Türrahmen wird nur getroffen, wenn er direkt vor dem Spieler ist
        }

        return false;
    }
}
