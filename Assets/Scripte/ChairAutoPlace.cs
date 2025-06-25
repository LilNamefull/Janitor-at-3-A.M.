// Datei: Assets/Scripte/ChairAutoPlace.cs
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
        Camera cam = Camera.main;
        if (cam == null) return;

        Ray ray = cam.ViewportPointToRay(Vector3.one * 0.5f);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.gameObject == gameObject && Input.GetKeyDown(interactKey))
            {
                Debug.Log("[ChairAutoPlace] E gedrückt auf Stuhl, starte Platzierung");
                StartCoroutine(PlaceRoutine());
            }
        }
    }

    private IEnumerator PlaceRoutine()
    {
        isPlaced = true;
        Collider col = GetComponent<Collider>();
        if (col) col.enabled = false;

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
        transform.position = endPos;
        transform.rotation = endRot;

        // Rufe Level1Manager auf
        if (Level1Manager.Instance != null)
        {
            Level1Manager.Instance.OnChairPlaced();
        }
        else
        {
            Debug.LogError("[ChairAutoPlace] Level1Manager.Instance ist null!");
        }
    }
}
