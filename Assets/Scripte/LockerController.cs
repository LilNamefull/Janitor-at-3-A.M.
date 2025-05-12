using UnityEngine;
using System.Collections;

public class LockerController : MonoBehaviour
{
    [Header("References")]
    public Transform doorTransform;      // Das eigentliche Tür-Objekt
    public Transform hingePoint;         // Leeres GameObject an der Scharnierlinie (Option A)
    public Vector3 hingeOffsetLocal;     // Alternativ Option B

    [Header("Animation")]
    public float openAngle = 60f;
    public float closeAngle = 0f;
    public float rotationSpeed = 180f;
    public float holdTime = 0.5f;

    [Header("Audio")]
    public AudioSource audioSource;

    private bool isLockering = false;

    public void StartLockering()
    {
        if (!isLockering)
            StartCoroutine(LockeringRoutine());
    }

    private IEnumerator LockeringRoutine()
    {
        isLockering = true;
        if (audioSource) { audioSource.loop = true; audioSource.Play(); }

        while (true)
        {
            yield return RotateTo(openAngle);
            yield return new WaitForSeconds(holdTime);
            yield return RotateTo(closeAngle);
            yield return new WaitForSeconds(holdTime);
        }
    }

    private IEnumerator RotateTo(float targetAngle)
    {
        // Bestimme den Hinge-Punkt im Welt-Raum
        Vector3 pivot = hingePoint != null
            ? hingePoint.position
            : doorTransform.position + doorTransform.TransformDirection(hingeOffsetLocal);

        // Ermittle aktuellen Winkel um Y-Achse relativ zum Pivot
        float currentAngle = doorTransform.localEulerAngles.y;
        // Rechne in den Bereich -180..180
        currentAngle = Mathf.DeltaAngle(0, currentAngle);

        // Zielwinkel
        float angle = currentAngle;
        while (Mathf.Abs(Mathf.DeltaAngle(angle, targetAngle)) > 0.5f)
        {
            // Berechne wie weit wir in diesem Frame drehen
            float step = rotationSpeed * Time.deltaTime;
            // Nächster Winkel näher an targetAngle
            angle = Mathf.MoveTowardsAngle(angle, targetAngle, step);
            // Rotation um den Hinge-Punkt: Differenz seit letztem Frame
            float deltaAngle = angle - doorTransform.localEulerAngles.y;
            doorTransform.RotateAround(pivot, Vector3.up, deltaAngle);
            yield return null;
        }

        // finalen Winkel exakt setzen
        float finalDelta = targetAngle - doorTransform.localEulerAngles.y;
        doorTransform.RotateAround(pivot, Vector3.up, finalDelta);
    }
}
