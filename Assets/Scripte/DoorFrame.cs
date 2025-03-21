using UnityEngine;

public class DoorFrame : MonoBehaviour
{
    public Door door; // Referenz zur Tür

    void Update()
    {
        // Überprüft, ob die E-Taste gedrückt wurde, um mit der Tür zu interagieren
        if (Input.GetKeyDown(KeyCode.E) && door != null)
        {
            door.Interact(); // Ruft die Interact-Methode der Tür auf, wenn sie vorhanden ist
        }
        else if (door == null)
        {
            Debug.LogError("Keine Tür zugewiesen im Inspector!"); // Gibt eine Fehlermeldung aus, falls keine Tür zugewiesen wurde
        }
    }
}
