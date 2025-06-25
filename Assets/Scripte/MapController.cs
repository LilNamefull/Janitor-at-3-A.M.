using UnityEngine;

public class MapController : MonoBehaviour
{
    [Tooltip("Referenz auf das UI-Panel, das die gesamte Karte enthält.")]
    public GameObject mapPanel;
    public GameObject hotbarUI;

    void Start()
    {
        // Stelle sicher, dass zu Spielstart die Map ausgeblendet ist
        if (mapPanel != null)
            mapPanel.SetActive(false);
        if (hotbarUI != null) hotbarUI.SetActive(true);
    }

    void Update()
    {
        if (mapPanel == null)
            return;

        // Wenn die Taste M gehalten wird, Karte anzeigen; sonst verstecken
        if (Input.GetKey(KeyCode.M))
        {
            if (hotbarUI != null)
                hotbarUI.SetActive(false);
            if (!mapPanel.activeSelf)
                mapPanel.SetActive(true);
        }
        else
        {
            if (mapPanel.activeSelf)
                mapPanel.SetActive(false);
            if (hotbarUI != null) hotbarUI.SetActive(true);
        }
    }
}
