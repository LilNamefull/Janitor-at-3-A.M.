using UnityEngine; // Importiert das UnityEngine-Namespace, um auf Unity-Funktionen und -Klassen zugreifen zu können.

public class Door : MonoBehaviour // Definiert eine Klasse "Door", die von MonoBehaviour erbt, um sie in Unity als Script verwenden zu können.
{
    private bool isOpen = false; // Eine boolesche Variable, die den aktuellen Zustand der Tür speichert (ob sie geöffnet oder geschlossen ist).
    public Transform doorTransform; // Referenz auf das Transform der Tür (das Objekt, das sich dreht). 
    public float openAngle = 180f; // Der Öffnungswinkel der Tür in Grad, standardmäßig 90 Grad.
    public float speed = 3f; // Die Geschwindigkeit, mit der die Tür sich öffnen oder schließen soll.

    private Quaternion closedRotation; // Eine Quaternion, die die Rotation der Tür im geschlossenen Zustand speichert.
    private Quaternion openRotation; // Eine Quaternion, die die Rotation der Tür im offenen Zustand speichert.
    private bool isMoving = false; // Eine boolesche Variable, die überprüft, ob die Tür gerade in Bewegung ist, um zu verhindern, dass mehrere Interaktionen gleichzeitig stattfinden.

    void Start() // Die Start-Methode wird einmal beim Start des Spiels aufgerufen.
    {
        closedRotation = doorTransform.localRotation; // Speichert die aktuelle Rotation der Tür als geschlossene Rotation.
        openRotation = closedRotation * Quaternion.Euler(0, openAngle, 0); // Berechnet die offene Rotation, indem die geschlossene Rotation um den angegebenen Winkel in der Y-Achse rotiert wird.
    }

    void Update() // Die Update-Methode wird jedes Frame aufgerufen.
    {
        if (Input.GetKeyDown(KeyCode.E) && !isMoving) // Wenn die "E"-Taste gedrückt wird und die Tür nicht in Bewegung ist, wird die Interaktion ausgelöst.
        {
            Interact(); // Ruft die Interact-Methode auf.
        }
    }

    public void Interact() // Diese Methode wird aufgerufen, wenn der Spieler mit der Tür interagieren möchte.
    {
        if (!isMoving) // Wenn die Tür nicht in Bewegung ist, kann eine neue Interaktion ausgeführt werden.
        {
            Debug.Log("Tür wird " + (isOpen ? "geschlossen" : "geöffnet")); // Gibt eine Nachricht aus, je nachdem, ob die Tür geöffnet oder geschlossen wird.
            StopAllCoroutines(); // Stoppt alle laufenden Coroutinen, um Konflikte zu vermeiden.
            StartCoroutine(RotateDoor(isOpen ? closedRotation : openRotation)); // Startet die Coroutine, um die Tür in die gewünschte Position (offen oder geschlossen) zu rotieren.
            isOpen = !isOpen; // Ändert den Zustand der Tür: Wenn sie offen war, wird sie geschlossen und umgekehrt.
        }
    }

    private System.Collections.IEnumerator RotateDoor(Quaternion targetRotation) // Eine Coroutine, die die Tür von der aktuellen Rotation zur Zielrotation bewegt.
    {
        isMoving = true; // Setzt die isMoving-Variable auf true, um zu signalisieren, dass sich die Tür in Bewegung befindet.
        while (Quaternion.Angle(doorTransform.localRotation, targetRotation) > 0.1f) // Solange die aktuelle Rotation von der Zielrotation mehr als 0.1 Grad abweicht.
        {
            doorTransform.localRotation = Quaternion.Slerp(doorTransform.localRotation, targetRotation, Time.deltaTime * speed); // Rotiert die Tür allmählich in Richtung der Zielrotation unter Verwendung von Slerp (Spherical Linear Interpolation).
            yield return null; // Wartet auf das nächste Frame, bevor die Coroutine fortgesetzt wird.
        }
        doorTransform.localRotation = targetRotation; // Setzt die Rotation der Tür exakt auf die Zielrotation, wenn der Unterschied klein genug ist.
        isMoving = false; // Setzt die isMoving-Variable auf false, um anzuzeigen, dass die Tür gestoppt ist und keine weiteren Bewegungen mehr stattfinden.
    }
}



