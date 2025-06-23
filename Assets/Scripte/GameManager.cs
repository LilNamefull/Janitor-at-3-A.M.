using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int keysCollected = 0;
    public int totalKeysRequired = 2;
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
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        hotbar = FindAnyObjectByType<Hotbar>();

        if (hotbar == null)
        {
            Debug.LogError("Hotbar nicht gefunden!");
        }

        if (keyCounterText != null)
        {
            keyCounterText.gameObject.SetActive(false);
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

        /*UpdateKeyCounterUI();

        // Tür entsperren wenn genug Schlüssel
        if (keysCollected >= totalKeysRequired && lockedDoor != null)
        {
            UnlockDoor();
        }*/
    }

    /*void UpdateKeyCounterUI()
    {
        if (keyCounterText == null)
            return;

        if (keysCollected > 0)
        {
            keyCounterText.gameObject.SetActive(true);
            keyCounterText.text = $"{keysCollected}/{totalKeysRequired} half of the scrolls";
        }
        else
        {
            // Keine Schlüssel: UI ausblenden
            keyCounterText.gameObject.SetActive(false);
        }
    }*/

    void UnlockDoor()
    {
        // Hier kannst du die Tür aufmachen, aktivieren, Animation etc.
        lockedDoor.SetActive(false); // Beispiel: Tür deaktivieren
        Debug.Log("Alle Schlüssel gesammelt! Tür ist jetzt offen.");
    }
    public void ResetKeys()
    {
        keysCollected = 0;
        keyInHotbarGiven = false;

        // Hotbar: ggf. Schlüssel-Icon entfernen. Falls Hotbar kein RemoveKey hat,
        // hier einen Reset implementieren oder Hotbar erweitern:
        if (hotbar != null)
        {
            // Wenn du in Hotbar ein RemoveKey oder ResetKey implementiert hast, rufe es hier auf:
            // hotbar.RemoveKey();
            // Ansonsten: du könntest z.B. neu instanziieren oder das vorhandene Flag manuell zurücksetzen,
            // falls Hotbar es unterstützt.
        }

        // UI ausblenden
        if (keyCounterText != null)
        {
            keyCounterText.gameObject.SetActive(false);
            keyCounterText.text = $"0/{totalKeysRequired} keys collected";
        }

        // Tür wieder verriegeln, falls Referenz gesetzt
        if (lockedDoor != null)
        {
            lockedDoor.SetActive(true);
        }

        Debug.Log("GameManager: Schlüssel-Zustand zurückgesetzt.");
    }

}
