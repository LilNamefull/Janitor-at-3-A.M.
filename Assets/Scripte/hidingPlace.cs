
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hidingPlace : MonoBehaviour
{
    [Header("UI & Spieler-Plätze")]
    public GameObject hideText;          // „Drücke E …“
    public GameObject stopHideText;      // „Drücke Q …“
    public GameObject normalPlayer;      // Dein Standard-Charakter
    public GameObject hidingPlayer;      // Dein Versteck-Charakter

    [Header("Monster & Logik")]
    public enemyAI monsterScript;        // Referenz zum enemyAI‐Script
    public Transform monsterTransform;   // Transform des Monsters
    public float loseDistance;           // Abstand, ab dem das Monster aufgegeben wird

    [Header("Audio")]
    public AudioSource hideSound;        // Sound beim Verstecken
    public AudioSource stopHideSound;    // Sound beim Herauskommen

    [Header("Raumerkennung")]
    public roomDetector detector;        // Prüft, ob du verstecken darfst

    [Header("Jumpscare (33 % Chance)")]
    [Tooltip("Prefab mit deinem Jumpscare-Bild (z. B. UI-Image oder 3D-Sprite)")]
    public GameObject jumpscarePrefab;
    [Tooltip("AudioSource mit Jumpscare-Sound")]
    public AudioSource jumpscareAudio;
    [Tooltip("Wartezeit nach dem Verstecken, bevor der Jumpscare erscheint")]
    public float jumpscareDelayAfterHide = 1f;
    [Tooltip("Dauer, wie lange das Jumpscare-Bild sichtbar bleiben soll")]
    public float jumpscareDuration = 1f;

    [Header("Jumpscare Camera")]
    [Tooltip("Ziehe hier die Kamera rein, vor der das Jumpscare-Bild erscheinen soll")]
    public Camera jumpscareCamera;

    [Header("Hotbar-Blockierung")]
    [Tooltip("Ziehe hier das GameObject mit deinem Hotbar-Skript (Hotbar.cs) hinein")]
    public Hotbar hotbarScript;
    [Tooltip("Ziehe hier das GameObject, das dein Hotbar-UI enthält (z. B. Canvas-Panel) hinein")]
    public GameObject hotbarUI;

    [Header("ExitPoint (außerhalb des Lockers)")]
    [Tooltip("Leeres GameObject, das die sichere Ausstiegsposition des Spielers markiert")]
    public Transform exitPoint;

    bool interactable = false;
    bool hiding = false;

    // Merkt sich das zuletzt aktive Hotbar-Item, damit wir es beim Verstecken ausblenden
    private GameObject _currentHotbarItem;

    void Start()
    {
        interactable = false;
        hiding = false;

        if (hideText != null) hideText.SetActive(false);
        if (stopHideText != null) stopHideText.SetActive(false);

        // Prüfe, ob exitPoint zugewiesen ist
        if (exitPoint == null)
        {
            Debug.LogError("[hidingPlace] Es wurde kein exitPoint gesetzt! Bitte im Inspector eine Transform für den ExitPoint zuweisen.");
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("MainCamera"))
        {
            if (detector != null && detector.inTrigger)
            {
                if (hideText != null) hideText.SetActive(true);
                interactable = true;
            }
            else
            {
                if (hideText != null) hideText.SetActive(false);
                interactable = false;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("MainCamera"))
        {
            if (hideText != null) hideText.SetActive(false);
            interactable = false;
        }
    }

    void Update()
    {
        // 1) Verstecken starten (Taste E)
        if (interactable && !hiding && Input.GetKeyDown(KeyCode.E))
        {
            if (hideText != null) hideText.SetActive(false);

            float rand = Random.value;
            Debug.Log($"[hidingPlace] Zufallswert für Jumpscare: {rand:F2}");
            // Zum Testen: rand = 0f, um Jumpscare immer auszulösen
            // float rand = 0f;

            if (rand < 0.33f && jumpscarePrefab != null && jumpscareAudio != null && jumpscareCamera != null)
            {
                Debug.Log("[hidingPlace] Jumpscare‐Branch gewählt");
                StartCoroutine(DoHideThenJumpscare());
            }
            else
            {
                Debug.Log("[hidingPlace] Kein Jumpscare, direkte DoHide()");
                DoHide();
            }
        }

        // 2) Herauskommen (Taste Q)
        if (hiding && Input.GetKeyDown(KeyCode.Q))
        {
            DoExit();
        }
    }

    /// <summary>
    /// Kern‐Logik: Spieler versteckt sich, Monster stoppen, Hotbar + Item ausblenden.
    /// </summary>
    void DoHide()
    {
        Debug.Log("[hidingPlace] DoHide() aufgerufen");

        // 1) Sound und Spieler‐Tausch
        if (hideSound != null) hideSound.Play();
        hidingPlayer.SetActive(true);

        // 2) Monster stoppen, falls es gerade jagen würde und genug Abstand
        float distance = Vector3.Distance(monsterTransform.position, normalPlayer.transform.position);
        if (distance > loseDistance && monsterScript != null && monsterScript.IsChasing)
        {
            // Abbrechen des Chase‐States
            monsterScript.CancelChase();
        }

        // 3) Hotbar blockieren: Script deaktivieren, UI unsichtbar machen
        if (hotbarScript != null) hotbarScript.enabled = false;
        if (hotbarUI != null) hotbarUI.SetActive(false);

        // 4) Aktuelles Hotbar‐Item (falls aktiv) ebenfalls ausblenden
        if (hotbarScript != null && hotbarScript.activeItem != null)
        {
            _currentHotbarItem = hotbarScript.activeItem;
            hotbarScript.activeItem.SetActive(false);
        }

        if (stopHideText != null) stopHideText.SetActive(true);
        hiding = true;
        normalPlayer.SetActive(false);
        interactable = false;
    }

    /// <summary>
    /// Führt den Exit durch: teleportiert den Spieler zum exitPoint und aktiviert Hotbar & normalen Player.
    /// </summary>
    void DoExit()
    {
        // 1) Teleportiere normalPlayer direkt auf exitPoint
        if (exitPoint != null)
        {
            normalPlayer.transform.position = exitPoint.position;
        }
        else
        {
            Debug.LogWarning("[hidingPlace] Kein exitPoint gesetzt, benutze gespeicherte Position (kann instabil sein).");
        }

        // 2) Hotbar‐Skript und UI wieder aktivieren
        if (hotbarScript != null) hotbarScript.enabled = true;
        if (hotbarUI != null) hotbarUI.SetActive(true);

        // 3) Falls ein Hotbar‐Item deaktiviert wurde, wieder aktivieren
        if (_currentHotbarItem != null)
            _currentHotbarItem.SetActive(true);

        // 4) Text & Sound, Player‐Tausch
        if (stopHideText != null) stopHideText.SetActive(false);
        if (stopHideSound != null) stopHideSound.Play();
        normalPlayer.SetActive(true);
        hidingPlayer.SetActive(false);
        hiding = false;
    }

    /// <summary>
    /// Zuerst verstecken (DoHide), dann nach einem Delay den Jumpscare anzeigen, danach das Objekt löschen.
    /// </summary>
    private IEnumerator DoHideThenJumpscare()
    {
        // 1) Sofort „hideen“ und Hotbar + Item deaktivieren
        DoHide();

        // 2) Wartezeit, bis der Spieler im Versteck-Modus ist
        yield return new WaitForSecondsRealtime(jumpscareDelayAfterHide);

        Debug.Log("[hidingPlace] Jumpscare jetzt anzeigen");

        // 3) Prüfen, ob Prefab/Camera/Audio gesetzt sind
        if (jumpscarePrefab == null)
        {
            Debug.LogError("[hidingPlace] jumpscarePrefab ist nicht gesetzt!");
            yield break;
        }
        if (jumpscareCamera == null)
        {
            Debug.LogError("[hidingPlace] jumpscareCamera ist nicht gesetzt!");
            yield break;
        }
        if (jumpscareAudio == null)
        {
            Debug.LogError("[hidingPlace] jumpscareAudio ist nicht gesetzt!");
            yield break;
        }

        // 4) Instanziere das Jumpscare-Prefab vor der Kamera
        GameObject js = Instantiate(jumpscarePrefab);
        js.transform.SetParent(jumpscareCamera.transform, false);
        js.transform.localPosition = new Vector3(0f, 0f, 1f);
        js.transform.localRotation = Quaternion.identity;
        Debug.Log("[hidingPlace] Jumpscare-Prefab instanziert");

        // 5) Jumpscare‐Sound abspielen
        jumpscareAudio.Play();
        Debug.Log("[hidingPlace] Jumpscare-Sound abgespielt");

        // 6) Warte für die Jumpscare-Dauer
        yield return new WaitForSecondsRealtime(jumpscareDuration);

        // 7) Entferne das Jumpscare-Objekt
        Destroy(js);
        Debug.Log("[hidingPlace] Jumpscare abgeschlossen");
    }
}
