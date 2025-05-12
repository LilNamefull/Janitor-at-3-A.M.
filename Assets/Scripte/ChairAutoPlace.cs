using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class ChairAutoPlace : MonoBehaviour
{
    [Header("Einstellungen")]
    public Transform targetSpot;       // Child-Objekt mit korrekter Pos/Rot am Tisch
    public float placeSpeed = 2f;      // Wie schnell der Stuhl zum Spot fährt
    public string interactKey = "e";   // Taste zum Auslösen

    private bool isPlaced = false;     // Nach Platzierung keine Interaktion mehr

    void Update()
    {
        if (isPlaced) return;

        // Ray aus der Mitte der Kamera
        Ray ray = Camera.main.ViewportPointToRay(Vector3.one * 0.5f);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Wenn wir gerade auf diesen Stuhl schauen und E drücken
            if (hit.collider.gameObject == gameObject && Input.GetKeyDown(interactKey))
            {
                StartCoroutine(PlaceRoutine());
            }
        }
    }

    private IEnumerator PlaceRoutine()
    {
        // Sperre weitere Interaktion
        isPlaced = true;
        // Deaktiviere Collider während Bewegung
        Collider col = GetComponent<Collider>();
        if (col) col.enabled = false;

        // Starte- und Zielwerte
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        Vector3 endPos = targetSpot.position;
        Quaternion endRot = targetSpot.rotation;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * placeSpeed;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }

        // Exakt ausrichten
        transform.position = endPos;
        transform.rotation = endRot;

        if (GameManagerIntro.Instance != null)
            GameManagerIntro.Instance.ChairPlaced();
        else
            Debug.LogError("GameManagerIntro.Instance ist null!");
    }
}
