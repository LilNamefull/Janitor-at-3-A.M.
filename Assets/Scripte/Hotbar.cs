using UnityEngine;
using UnityEngine.UI;

public class Hotbar : MonoBehaviour
{
    public Transform cameraTransform;
    public GameObject mopPrefab;
    public GameObject flashlightPrefab;
    public GameObject keyPrefab;

    public Image[] hotbarSlots; // UI-Symbole der Hotbar (Ziehe sie in Unity in das Array)
    public Color normalColor = new Color(1, 1, 1, 0.5f); // Standardfarbe (Transparent)
    public Color selectedColor = new Color(1, 1, 1, 1f); // Volle Sichtbarkeit für das aktive Item

    private GameObject activeItem;
    private int currentSlot = 0;
    private bool hasKey = false;

    void Start()
    {
        EquipItem(0);
    }

    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll > 0f)
        {
            ChangeSlot(-1);
        }
        else if (scroll < 0f)
        {
            ChangeSlot(1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha1)) EquipItem(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) EquipItem(1);
        if (Input.GetKeyDown(KeyCode.Alpha3) && hasKey) EquipItem(2);
    }

    public void PickupKey()
    {
        hasKey = true;
    }

    void ChangeSlot(int direction)
    {
        int newSlot = currentSlot + direction;

        if (newSlot > 2) newSlot = 0;
        if (newSlot < 0) newSlot = 2;

        if (newSlot == 2 && !hasKey) return;

        EquipItem(newSlot);
    }

    void EquipItem(int slot)
    {
        if (activeItem != null) Destroy(activeItem);

        currentSlot = slot;
        GameObject selectedPrefab = null;

        switch (slot)
        {
            case 0: selectedPrefab = mopPrefab; break;
            case 1: selectedPrefab = flashlightPrefab; break;
            case 2: if (hasKey) selectedPrefab = keyPrefab; break;
        }

        if (selectedPrefab != null)
        {
            activeItem = Instantiate(selectedPrefab);
            HotbarItemFollow followScript = activeItem.AddComponent<HotbarItemFollow>();
            followScript.cameraTransform = cameraTransform;

            switch (slot)
            {
                case 0:
                    followScript.positionOffset = new Vector3(0.5f, -0.75f, 2f);
                    followScript.rotationOffset = new Vector3(-40f, 0, 45f);
                    break;

                case 1:
                    followScript.positionOffset = new Vector3(0.7f, -0.3f, 0.5f);
                    followScript.rotationOffset = new Vector3(0, 83f, 0);
                    break;

                case 2:
                    followScript.positionOffset = new Vector3(0.3f, -0.2f, 0.5f);
                    followScript.rotationOffset = new Vector3(0, 0, 0);
                    break;
            }
        }

        UpdateHotbarUI();
    }

    void UpdateHotbarUI()
    {
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            if (i == currentSlot)
                hotbarSlots[i].color = selectedColor; // Aktives Item hervorheben
            else
                hotbarSlots[i].color = normalColor; // Alle anderen verblassen
        }
    }
}
