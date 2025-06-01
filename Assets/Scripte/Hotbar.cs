using UnityEngine;
using UnityEngine.UI;

public class Hotbar : MonoBehaviour
{
    [Header("Camera Reference")]
    public Transform cameraTransform;

    [Header("Item Prefabs")]
    public GameObject mopPrefab;
    public GameObject flashlightPrefab;
    public GameObject keyPrefab;

    [Header("UI Slots")]
    public Image[] hotbarSlots;               // Array der UI‐Symbole (Slots)
    public Color normalColor = new Color(1, 1, 1, 0.5f);  // Ausgeblasene Farbe
    public Color selectedColor = new Color(1, 1, 1, 1f);  // Volle Deckkraft

    [HideInInspector]
    public GameObject activeItem;             // Aktuell in der Hand gehaltenes Item

    private int currentSlot = 0;              // Welcher Slot ist gerade aktiv? (0=Mop, 1=Flashlight, 2=Key)
    private bool hasKey = false;              // Haben wir schon den Schlüssel aufgehoben?


    public bool IsMopEquipped()
    {
        return currentSlot == 0; // 0 = Mop
    }

    void Start()
    {
        // Beim Start immer den ersten Slot ausrüsten (Mop)
        EquipItem(0);
    }

    void Update()
    {
        // Mausrad‐Wechsel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f) ChangeSlot(-1);
        else if (scroll < 0f) ChangeSlot(1);

        // Zahlentasten‐Wechsel
        if (Input.GetKeyDown(KeyCode.Alpha1)) EquipItem(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) EquipItem(1);
        if (Input.GetKeyDown(KeyCode.Alpha3) && hasKey) EquipItem(2);
    }

    public void PickupKey()
    {
        hasKey = true;
        // Sobald wir den Schlüssel bekamen, könnten wir auch direkt Slot 3 freigeben oder Icon anzeigen.
    }

    private void ChangeSlot(int direction)
    {
        int newSlot = currentSlot + direction;
        if (newSlot > 2) newSlot = 0;
        if (newSlot < 0) newSlot = 2;
        if (newSlot == 2 && !hasKey) return;  // Schlüssel‐Slot nur, wenn wir hasKey == true

        EquipItem(newSlot);
    }

    private void EquipItem(int slot)
    {
        // 1) Alte aktive Item‐Instanz löschen (sofern vorhanden)
        if (activeItem != null)
        {
            Destroy(activeItem);
            activeItem = null;
        }

        currentSlot = slot;
        GameObject selectedPrefab = null;

        switch (slot)
        {
            case 0:
                selectedPrefab = mopPrefab;
                break;
            case 1:
                selectedPrefab = flashlightPrefab;
                break;
            case 2:
                if (hasKey) selectedPrefab = keyPrefab;
                break;
        }

        if (selectedPrefab != null)
        {
            // 2) Neues Item instanziieren
            activeItem = Instantiate(selectedPrefab);

            // 3) Item direkt als Child unter die Kamera hängen, damit es sich mitdreht
            activeItem.transform.SetParent(cameraTransform, false);

            // 4) Position und Rotation relativ zur Kamera setzen
            switch (slot)
            {
                case 0: // Mop
                    activeItem.transform.localPosition = new Vector3(0.5f, -0.75f, 2f);
                    activeItem.transform.localEulerAngles = new Vector3(-40f, 0f, 45f);
                    break;
                case 1: // Taschenlampe
                    activeItem.transform.localPosition = new Vector3(0.7f, -0.3f, 0.5f);
                    activeItem.transform.localEulerAngles = new Vector3(0f, 83f, 0f);
                    break;
                case 2: // Schlüssel
                    activeItem.transform.localPosition = new Vector3(1.5f, -0.9f, 2f);
                    activeItem.transform.localEulerAngles = new Vector3(0f, -40f, 0f);
                    break;
            }
        }

        // 5) UI‐Slots aktualisieren
        UpdateHotbarUI();
    }

    private void UpdateHotbarUI()
    {
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            hotbarSlots[i].color = (i == currentSlot) ? selectedColor : normalColor;
        }
    }
}


