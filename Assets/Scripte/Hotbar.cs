using UnityEngine;
using UnityEngine.UI;

public class Hotbar : MonoBehaviour
{
    public Image[] slotIcons;  // Die Bilder für die Slots
    public GameObject mopPrefab;
    public GameObject flashlightPrefab;
    public GameObject keyPrefab;

    private GameObject currentItem;
    private int selectedSlot = 0;

    void Start()
    {
        SelectSlot(0);  // Startet mit Slot 1
    }

    void Update()
    {
        // Wechseln mit Mausrad
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
        {
            SelectSlot((selectedSlot + 1) % 3);
        }
        else if (scroll < 0f)
        {
            SelectSlot((selectedSlot + 2) % 3); // Rückwärts durch die Slots
        }

        // Item verwenden
        if (Input.GetKeyDown(KeyCode.E))
        {
            UseItem();
        }
    }

    void SelectSlot(int slotIndex)
    {
        selectedSlot = slotIndex;

        // UI hervorheben (optional)
        for (int i = 0; i < slotIcons.Length; i++)
        {
            slotIcons[i].color = (i == selectedSlot) ? Color.white : Color.gray;
        }
    }

    void UseItem()
    {
        if (currentItem != null)
        {
            Destroy(currentItem);
        }

        switch (selectedSlot)
        {
            case 0: // Wischmop
                currentItem = Instantiate(mopPrefab, transform);
                break;
            case 1: // Taschenlampe
                currentItem = Instantiate(flashlightPrefab, transform);
                break;
            case 2: // Schlüssel
                currentItem = Instantiate(keyPrefab, transform);
                break;
        }
    }
}

