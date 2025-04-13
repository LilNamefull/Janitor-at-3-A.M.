using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int keysCollected = 0;
    public int totalKeysRequired = 4;
    public TextMeshProUGUI keyCounterText;  // UI-Text für Schlüsselanzeige
    public GameObject lockedDoor; // Referenz zur verschlossenen Tür

    private bool keyInHotbarGiven = false;
    private Hotbar hotbar;

    void Awake()
    {
        // Singleton-Pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        hotbar = FindAnyObjectByType<Hotbar>();

        if (hotbar == null)
        {
            Debug.LogError("Hotbar nicht gefunden!");
        }
    }

    public void CollectKey()
    {
        keysCollected++;

        // Hotbar nur beim ersten Schlüssel aktivieren
        if (!keyInHotbarGiven && hotbar != null)
        {
            hotbar.PickupKey();
            keyInHotbarGiven = true;
        }

        UpdateKeyCounterUI();

        // Tür entsperren wenn genug Schlüssel
        if (keysCollected >= totalKeysRequired && lockedDoor != null)
        {
            UnlockDoor();
        }
    }

    void UpdateKeyCounterUI()
    {
        if (keyCounterText != null)
        {
            keyCounterText.text = $"{keysCollected}/{totalKeysRequired} Keys";
        }
    }

    void UnlockDoor()
    {
        // Hier kannst du die Tür aufmachen, aktivieren, Animation etc.
        lockedDoor.SetActive(false); // Beispiel: Tür deaktivieren
        Debug.Log("Alle Schlüssel gesammelt! Tür ist jetzt offen.");
    }
}
