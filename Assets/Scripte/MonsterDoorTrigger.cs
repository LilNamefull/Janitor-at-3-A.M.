using System.Collections;
using UnityEngine;

public class MonsterDoorTrigger : MonoBehaviour
{
    public Transform doorPivot;               // Das Tür-Schanier (Pivot)
    public float openAngle = 90f;             // Wie weit sich die Tür öffnet
    public float closedAngle = 0f;            // Wo die Tür startet
    public float rotationSpeed = 100f;        // Wie schnell sich die Tür dreht
    public float stayOpenTime = 5f;           // Wie lange die Tür offen bleibt

    private bool isOpen = false;
    private bool isMoving = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Monster") && !isOpen && !isMoving)
        {
            StartCoroutine(OpenAndCloseRoutine());
        }
    }

    IEnumerator OpenAndCloseRoutine()
    {
        isMoving = true;
        yield return StartCoroutine(RotateDoor(openAngle));      // Öffnen
        isOpen = true;

        yield return new WaitForSeconds(stayOpenTime);           // Offen bleiben

        yield return StartCoroutine(RotateDoor(closedAngle));    // Schließen
        isOpen = false;
        isMoving = false;
    }

    IEnumerator RotateDoor(float targetY)
    {
        while (Mathf.Abs(Mathf.DeltaAngle(doorPivot.localEulerAngles.y, targetY)) > 0.1f)
        {
            float y = Mathf.MoveTowardsAngle(doorPivot.localEulerAngles.y, targetY, rotationSpeed * Time.deltaTime);
            Vector3 currentRotation = doorPivot.localEulerAngles;
            doorPivot.localEulerAngles = new Vector3(currentRotation.x, y, currentRotation.z);
            yield return null;
        }
    }
}
