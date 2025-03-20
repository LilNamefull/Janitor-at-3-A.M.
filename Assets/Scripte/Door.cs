using UnityEngine;
using System.Collections;

public class Door : Interactable
{
    private bool isOpen = false;
    public Transform doorTransform; // Referenz zur Tür (Kind-Objekt)
    public float openAngle = 90f; // Winkel zum Öffnen
    public float speed = 3f; // Öffnungsgeschwindigkeit

    private Quaternion closedRotation;
    private Quaternion openRotation;

    void Start()
    {
        closedRotation = doorTransform.rotation;
        openRotation = Quaternion.Euler(0, doorTransform.eulerAngles.y + openAngle, 0);
    }

    public override void Interact()
    {
        Debug.Log("Tür wird " + (isOpen ? "geschlossen" : "geöffnet"));

        StopAllCoroutines();
        StartCoroutine(RotateDoor(isOpen ? closedRotation : openRotation));
        doorTransform.rotation = isOpen ? openRotation : closedRotation;
        if (isOpen)
        {
            doorTransform.Rotate(0, -openAngle, 0);
        }
        else
        {
            doorTransform.Rotate(0, openAngle, 0);
        }

        isOpen = !isOpen;
        
    }

    private IEnumerator RotateDoor(Quaternion targetRotation)
    {
        Quaternion startRotation = doorTransform.rotation;
        float time = 0;

        Debug.Log("Start Rotation: " + startRotation.eulerAngles);
        Debug.Log("Target Rotation: " + targetRotation.eulerAngles);

        while (time < 1)
        {
            time += Time.deltaTime * speed;
            doorTransform.rotation = Quaternion.Lerp(startRotation, targetRotation, time);

            Debug.Log("Aktuelle Rotation: " + doorTransform.eulerAngles);
            yield return null;
        }

        doorTransform.rotation = targetRotation;
        Debug.Log("Endgültige Rotation: " + doorTransform.eulerAngles);
    }


}
