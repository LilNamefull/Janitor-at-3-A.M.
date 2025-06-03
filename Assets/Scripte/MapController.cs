using UnityEngine;

public class MapController : MonoBehaviour
{
    [Tooltip("Referenz auf das UI-Panel, das die gesamte Karte enthält.")]
    public GameObject mapPanel;

    void Start()
    {
        // Stelle sicher, dass zu Spielstart die Map ausgeblendet ist
        if (mapPanel != null)
            mapPanel.SetActive(false);
    }

    void Update()
    {
        if (mapPanel == null)
            return;

        // Wenn die Taste T gehalten wird, Karte anzeigen; sonst verstecken
        if (Input.GetKey(KeyCode.T))
        {
            if (!mapPanel.activeSelf)
                mapPanel.SetActive(true);
        }
        else
        {
            if (mapPanel.activeSelf)
                mapPanel.SetActive(false);
        }
    }
}
