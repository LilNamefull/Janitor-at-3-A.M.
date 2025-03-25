using System.Collections.Generic;
using UnityEngine;

public class Hotbar : MonoBehaviour
{
    public List<GameObject> itemPrefabs;  // Liste der Prefabs für Wischmop, Taschenlampe, Schlüssel
    public Transform itemSpawnPoint;      // Wo das Item erscheinen soll (z. B. vor der Kamera)

    private GameObject currentItem;       // Das aktuell instanziierte Item
    private int currentSlot = 0;          // Aktuell ausgewählter Slot

    void Start()
    {
        SpawnItem();  // Erstes Item direkt spawnen
    }

    void Update()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            ChangeSlot(1);
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            ChangeSlot(-1);
        }
    }

    void ChangeSlot(int direction)
    {
        currentSlot += direction;

        if (currentSlot >= itemPrefabs.Count)
            currentSlot = 0;
        if (currentSlot < 0)
            currentSlot = itemPrefabs.Count - 1;

        SpawnItem();
    }

    void SpawnItem()
    {
        // Falls schon ein Item existiert, löschen
        if (currentItem != null)
        {
            Destroy(currentItem);
        }

        // Falls der Slot leer ist (z. B. für den Schlüssel, wenn noch nicht eingesammelt), nichts tun
        if (itemPrefabs[currentSlot] == null)
        {
            return;
        }

        // Neues Item aus dem Prefab instanziieren
        currentItem = Instantiate(itemPrefabs[currentSlot], itemSpawnPoint.position, itemSpawnPoint.rotation);
        currentItem.transform.SetParent(itemSpawnPoint);

        // INDIVIDUELLE POSITIONEN & ROTATIONEN:
        switch (currentSlot)
        {
            case 0: // Wischmop
                currentItem.transform.localPosition = new Vector3(-0.072f, -1.5f, 3.267f);  // Leicht nach vorne & rechts
                currentItem.transform.localRotation = Quaternion.Euler(27.653f, 100.669f, -60.62f);  // Drehe um 90° nach rechts
                break;

            case 1: // Taschenlampe
                currentItem.transform.localPosition = new Vector3(-0.646f, -0.374f, 0.352f);  // Etwas tiefer & nach vorne
                currentItem.transform.localRotation = Quaternion.Euler(1.924f, -268.106f, 0.587f);  // Standard-Ausrichtung
                break;

            case 2: // Schlüssel
                currentItem.transform.localPosition = new Vector3(0.1f, -0.1f, 0.5f);  // Leicht nach vorne
                currentItem.transform.localRotation = Quaternion.Euler(0, -45, 0);  // Leichte Drehung
                break;
        }
    }

    // Funktion, um den Schlüssel später im Spiel hinzuzufügen
    public void AddKeyToHotbar(GameObject keyPrefab)
    {
        itemPrefabs[2] = keyPrefab; // Slot 2 (Index 2) ist für den Schlüssel reserviert
    }
}
