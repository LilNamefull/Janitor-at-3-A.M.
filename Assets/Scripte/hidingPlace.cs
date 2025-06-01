using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hidingPlace : MonoBehaviour
{
    [Header("UI & Spieler-Plätze")]
    public GameObject hideText;          // "Drücke E …"
    public GameObject stopHideText;      // "Drücke Q …"
    public GameObject normalPlayer;      // Dein Standard-Charakter
    public GameObject hidingPlayer;      // Dein Versteck-Charakter
    public enemyAI monsterScript;        // Dein Enemy-Script
    public Transform monsterTransform;   // Monster-Transform
    public float loseDistance;           // Abstand, um Chase abzubrechen

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
    public Hotbar hotbarScript;          // ACHTUNG: jetzt konket vom Typ Hotbar
    [Tooltip("Ziehe hier das GameObject, das dein Hotbar-UI enthält (z. B. Canvas-Panel) hinein")]
    public GameObject hotbarUI;

    private Vector3 _savedPlayerPosition;

    bool interactable = false;
    bool hiding = false;

    // Wir merken uns hier, welches Item in der Hand gerade ausge­rüstet ist,
    // damit wir es beim Verstecken deaktivieren und beim Herauskommen wieder aktivieren können:
    private GameObject _currentHotbarItem;

    void Start()
    {
        interactable = false;
        hiding = false;

        if (hideText != null) hideText.SetActive(false);
        if (stopHideText != null) stopHideText.SetActive(false);
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
            // Zum Testen: setz rand = 0f, um Jumpscare immer auszulösen.
            // float rand = 0f;

            if (rand < 0.33f && jumpscarePrefab != null && jumpscareAudio != null && jumpscareCamera != null)
            {
                Debug.Log("[hidingPlace] Jumpscare-Branch gewählt");
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
            normalPlayer.transform.position = _savedPlayerPosition;

            // 2a) Hotbar-Skript und UI wieder aktivieren:
            if (hotbarScript != null) hotbarScript.enabled = true;
            if (hotbarUI != null) hotbarUI.SetActive(true);

            // 2b) Falls es ein aktives Hotbar-Item gab, machen wir es wieder sichtbar:
            if (_currentHotbarItem != null)
                _currentHotbarItem.SetActive(true);

            // 2c) Rest: Text und Sound, Player-Tausch
            if (stopHideText != null) stopHideText.SetActive(false);
            if (stopHideSound != null) stopHideSound.Play();
            normalPlayer.SetActive(true);
            hidingPlayer.SetActive(false);
            hiding = false;
        }
    }

    /// <summary>
    /// Hier deaktivieren wir alles (inklusive Hotbar-Item), wenn der Spieler sich versteckt.
    /// </summary>
    void DoHide()
    {
        Debug.Log("[hidingPlace] DoHide() aufgerufen");

        _savedPlayerPosition = normalPlayer.transform.position;

        // 1) Abspielen des Versteck-Sounds und Aktivieren der hidingPlayer-Perspektive:
        if (hideSound != null) hideSound.Play();
        hidingPlayer.SetActive(true);

        // 2) Monster stoppen, wenn es jagt und der Abstand groß genug ist:
        float distance = Vector3.Distance(monsterTransform.position, normalPlayer.transform.position);
        if (distance > loseDistance && monsterScript != null && monsterScript.chasing)
        {
            monsterScript.stopChase();
        }

        // 3) Hotbar‐Skript und Hotbar‐UI deaktivieren:
        if (hotbarScript != null) hotbarScript.enabled = false;
        if (hotbarUI != null) hotbarUI.SetActive(false);

        // 4) „activeItem“ aus dem Hotbar-Skript (falls gesetzt) ausblenden:
        if (hotbarScript != null && hotbarScript.activeItem != null)
        {
            _currentHotbarItem = hotbarScript.activeItem;
            hotbarScript.activeItem.SetActive(false);
        }

        // 5) Letzte UI-Hinweise und Flag:
        if (stopHideText != null) stopHideText.SetActive(true);
        hiding = true;
        normalPlayer.SetActive(false);
        interactable = false;
    }

    /// <summary>
    /// Zuerst verstecken, dann nach einem Delay kommt der Jumpscare, danach wird das Jumpscare-Objekt wieder zerstört.
    /// </summary>
    private IEnumerator DoHideThenJumpscare()
    {
        // 1) Sofort „hideen“ und Hotbar + Item deaktivieren
        DoHide();

        // 2) Kurzes Warten (sodass man „richtig“ im Locker-Modus ist):
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

        // 4) Instanziieren des Jumpscare-Prefabs vor der Kamera
        GameObject js = Instantiate(jumpscarePrefab);
        js.transform.SetParent(jumpscareCamera.transform, false);
        js.transform.localPosition = new Vector3(0f, 0f, 1f);
        js.transform.localRotation = Quaternion.identity;
        Debug.Log("[hidingPlace] Jumpscare-Prefab instanziert");

        // 5) Jumpscare-Sound abspielen
        jumpscareAudio.Play();
        Debug.Log("[hidingPlace] Jumpscare-Sound abgespielt");

        // 6) Warten für die Jumpscare-Dauer
        yield return new WaitForSecondsRealtime(jumpscareDuration);

        // 7) Jumpscare-Objekt entfernen
        Destroy(js);
        Debug.Log("[hidingPlace] Jumpscare abgeschlossen");
    }
}
