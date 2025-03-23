using UnityEngine;

public class Door : MonoBehaviour
{
    private bool isOpen = false;
    public Transform doorTransform; // Die Tür selbst
    public float openAngle = 90f; // Öffnungswinkel
    public float speed = 3f; // Geschwindigkeit
    public Transform player; // Referenz zum Spieler
    public float interactionDistance = 3f; // Maximale Distanz für Interaktion
    public LayerMask interactableLayer; // Nur bestimmte Objekte als Tür erkennen

    private Quaternion closedRotation;
    private Quaternion openRotation;
    private bool isMoving = false; // Blockiert neue Eingaben während der Animation

    void Start()
    {
        closedRotation = doorTransform.localRotation;
        openRotation = closedRotation * Quaternion.Euler(0, openAngle, 0);

        if (player == null) // Falls der Spieler nicht manuell zugewiesen wurde, suche ihn
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && !isMoving && IsPlayerLookingAtDoor())
        {
            Interact();
        }
    }

    public void Interact()
    {
        if (!isMoving)
        {
            Debug.Log("Tür wird " + (isOpen ? "geschlossen" : "geöffnet"));
            StopAllCoroutines();
            StartCoroutine(RotateDoor(isOpen ? closedRotation : openRotation));
            isOpen = !isOpen;
        }
    }

    private System.Collections.IEnumerator RotateDoor(Quaternion targetRotation)
    {
        isMoving = true;
        while (Quaternion.Angle(doorTransform.localRotation, targetRotation) > 0.1f)
        {
            doorTransform.localRotation = Quaternion.Slerp(doorTransform.localRotation, targetRotation, Time.deltaTime * speed);
            yield return null;
        }
        doorTransform.localRotation = targetRotation;
        isMoving = false;
    }

    private bool IsPlayerLookingAtDoor()
    {
        if (player == null) return false;

        // Prüfe die Entfernung
        float distance = Vector3.Distance(player.position, transform.position);
        if (distance > interactionDistance) return false;

        // Prüfe, ob der Spieler auf die Tür schaut (Raycast)
        Ray ray = new Ray(player.position, player.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
        {
            return hit.transform == transform; // Tür wird nur getroffen, wenn sie direkt vor dem Spieler ist
        }

        return false;
    }
}


