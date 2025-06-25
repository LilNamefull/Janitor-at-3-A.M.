using UnityEngine;
using System.Collections;

public class CleanableSpot : MonoBehaviour
{
    [Header("Cleaning Settings")]
    public float cleanDuration = 4f;          // Zeit in Sekunden, um den Spot wegzuwischen
    public Vector3 initialScale = Vector3.one; // Start-Skalierung
    public Vector3 minScale = Vector3.zero;    // End-Skalierung (0 = verschwunden)

    private float cleanTimer = 0f;
    private bool isCleaning = false;
    private Hotbar hotbar;
    private Camera cam;

    void Start()
    {
        transform.localScale = initialScale;
        hotbar = FindAnyObjectByType<Hotbar>();
        cam = Camera.main;
        if (hotbar == null) Debug.LogError("Hotbar nicht gefunden!");
    }

    void Update()
    {
        if (isCleaning)
        {
            cleanTimer += Time.deltaTime;
            float t = Mathf.Clamp01(cleanTimer / cleanDuration);
            transform.localScale = Vector3.Lerp(initialScale, minScale, t);

            if (cleanTimer >= cleanDuration)
            {
                Destroy(gameObject);
                if (Level1Manager.Instance != null)
                    Level1Manager.Instance.OnSpotCleaned();
                else
                    Debug.LogError("[CleanableSpot] Level1Manager.Instance ist null!");
            }
        }
    

        if (Input.GetMouseButton(0) && hotbar != null && hotbar.IsMopEquipped())
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.gameObject == gameObject)
            {
                isCleaning = true;
            }
            else
            {
                isCleaning = false;
            }
        }
        else
        {
            isCleaning = false;
        }
    }
}

