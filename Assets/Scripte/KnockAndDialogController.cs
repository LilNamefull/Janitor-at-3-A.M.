using UnityEngine;
using System.Collections;

public class KnockAndDialogController : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource knockAudio;       // 3D AudioSource am Locker
    public AudioSource backgroundMusic;

    [Header("Ranges")]
    public float knockMaxDistance = 12f; // Hörweite
    public float knockMinDistance = 2f;  // Ab hier pausiert
    public float interactDistance = 3f;  // Ab hier E-Interaktion möglich

    [Header("Dialog")]
    public string[] dialogAfterOpen;     // Dialog 1 nach E-Drücken
    public string[] dialogNPC;           // Dialog 2 mit NPC
    public GameObject npcPrefab;         // NPC-Prefab
    public Transform npcSpawnPoint;      // NPC-Spawn-Position

    [Header("Timings")]
    public float dialogDelay = 0.5f;     // Warte vor Dialog 1
    public float lookSpeed = 2f;         // Drehtempo

    public GameObject playerMain;
    public Camera playerCamera;
    public Camera cinematicCamera;

    [Header("Monster Door")]
    [Tooltip("Zieh hier dein Monster-Tür-GameObject hinein")]
    public GameObject monsterDoor;
    public GameObject MonsterDoorFrameWithoutcode;
    public GameObject Invinsiblewallbefor;
    public GameObject Invinsiblewallmidlele;
    public GameObject Invinsiblewallafter;
    public GameObject Invinsiblewallbefor2;
    public GameObject Invinsiblewallmidlele2;
    public GameObject Invinsiblewallafter2;

    [Header("UI")]
    public GameObject hotbarUI;

    [Header("Auto-Dialog nach Klopfen")]
    [Tooltip("Sekunden nach Start des Klopfens, bis automatisch der Auto-Dialog mit diesen Zeilen startet")]
    public float autoDialogDelay = 5f;

    [Tooltip("Dialogzeilen, die nach autoDialogDelay angezeigt werden (z.B. 2 Zeilen)")]
    public string[] autoDialogLines;

    private Coroutine autoDialogCoroutine = null;
    private bool autoDialogShown = false; // Flag, damit es nur einmal passiert

    private Transform player;
    private bool isKnocking = false;
    private bool cutsceneStarted = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (playerMain != null) playerMain.gameObject.SetActive(true);

        if (knockAudio == null) Debug.LogError("knockAudio fehlt!");
        if (hotbarUI != null) hotbarUI.SetActive(true);

        if (playerCamera != null) playerCamera.gameObject.SetActive(true);
        if (cinematicCamera != null) cinematicCamera.gameObject.SetActive(false);

        // Monster-Tür initial deaktivieren
        if (monsterDoor != null)
            monsterDoor.SetActive(false);
        if (MonsterDoorFrameWithoutcode != null)
            MonsterDoorFrameWithoutcode.SetActive(true);
        if (Invinsiblewallmidlele != null)
            Invinsiblewallmidlele.SetActive(false);
        if (Invinsiblewallafter != null)
            Invinsiblewallafter.SetActive(false);
        if (Invinsiblewallmidlele2 != null)
            Invinsiblewallmidlele2.SetActive(false);
        if (Invinsiblewallafter2 != null)
            Invinsiblewallafter2.SetActive(false);

        // AudioSource konfigurieren
        knockAudio.spatialBlend = 1f;
        knockAudio.loop = true;
        knockAudio.playOnAwake = false;
        knockAudio.minDistance = knockMinDistance;
        knockAudio.maxDistance = knockMaxDistance;
    }

    void Update()
    {
        if (player == null) return;

        // 1) Starte Klopfen, sobald alle Aufgaben erledigt sind
        if (!isKnocking && GameManagerIntro.Instance.allTasksDone)
        {
            if (backgroundMusic != null && backgroundMusic.isPlaying)
                backgroundMusic.Stop();

            knockAudio.Play();
            isKnocking = true;
            cutsceneStarted = false;
            autoDialogShown = false;
            if (autoDialogCoroutine != null)
            {
                StopCoroutine(autoDialogCoroutine);
                autoDialogCoroutine = null;
            }
            autoDialogCoroutine = StartCoroutine(AutoTriggerDialogAfterDelay());

            // Wände umschalten wie bisher
            if (Invinsiblewallbefor != null)
                Invinsiblewallbefor.SetActive(false);
            if (Invinsiblewallmidlele != null)
                Invinsiblewallmidlele.SetActive(true);
            if (Invinsiblewallbefor2 != null)
                Invinsiblewallbefor2.SetActive(false);
            if (Invinsiblewallmidlele2 != null)
                Invinsiblewallmidlele2.SetActive(true);
        }

        if (isKnocking)
        {
            // 2) Pause/UnPause basierend auf Spieler-Abstand
            float dist = Vector3.Distance(player.position, knockAudio.transform.position);
            if (dist <= knockMinDistance && knockAudio.isPlaying)
                knockAudio.Pause();
            else if (dist > knockMinDistance && !knockAudio.isPlaying)
                knockAudio.UnPause();

            // 3) E-Interaktion, wenn nah genug
            if (dist <= interactDistance && !cutsceneStarted && Input.GetKeyDown(KeyCode.E))
            {
                cutsceneStarted = true;

                // Stoppe automatischen Dialog, falls noch aktiv
                if (autoDialogCoroutine != null)
                {
                    StopCoroutine(autoDialogCoroutine);
                    autoDialogCoroutine = null;
                }

                knockAudio.Stop();

                // 1) Collider ausschalten
                var col = GetComponent<Collider>();
                if (col != null)
                    col.enabled = false;

                // 2) Interactable-Script ausschalten (nur zur Sicherheit)
                var interactable = GetComponent<Interactable>();
                if (interactable != null)
                    interactable.enabled = false;

                if (GameManagerIntro.Instance.spotsText != null)
                    GameManagerIntro.Instance.spotsText.gameObject.SetActive(false);
                if (GameManagerIntro.Instance.chairsText != null)
                    GameManagerIntro.Instance.chairsText.gameObject.SetActive(false);

                if (hotbarUI != null)
                    hotbarUI.SetActive(false);

                StartCoroutine(DialogSequence());
            }
        }
    }

    // === NEU: Coroutine für automatischen Trigger des Dialogs nach Delay ===
    private IEnumerator AutoTriggerDialogAfterDelay()
    {
        yield return new WaitForSecondsRealtime(autoDialogDelay);
        autoDialogCoroutine = null;

        if (isKnocking && !cutsceneStarted && !autoDialogShown)
        {
            autoDialogShown = true;
            // Pausiere Klopf-Audio während Auto-Dialog
            knockAudio.Pause();

            // Freeze Game und Cursor freigeben
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Starte Auto-Dialog
            if (autoDialogLines != null && autoDialogLines.Length > 0)
            {
                if (hotbarUI != null)
                    hotbarUI.SetActive(false);
                DialogueManager.Instance.exitButton.gameObject.SetActive(false);
                DialogueManager.Instance.StartDialogue(autoDialogLines);
                yield return new WaitUntil(() => !DialogueManager.Instance.IsInDialogue);
            }
            else
            {
                Debug.LogWarning("[KnockAndDialogController] autoDialogLines nicht gesetzt oder leer.");
            }

            // Nach Auto-Dialog: unfreeze, Cursor sperren, Klopf-Audio fortsetzen
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (knockAudio != null)
                knockAudio.UnPause();
            if (hotbarUI != null)
                hotbarUI.SetActive(true);
        }
    }

    private IEnumerator DialogSequence()
    {
        // a) Warte vor erstem Dialog
        yield return new WaitForSecondsRealtime(dialogDelay);

        // b) Dialog 1 starten
        DialogueManager.Instance.exitButton.gameObject.SetActive(false);
        DialogueManager.Instance.StartDialogue(dialogAfterOpen);
        yield return new WaitUntil(() => !DialogueManager.Instance.IsInDialogue);

        // c) NPC-Dialog
        DialogueManager.Instance.exitButton.gameObject.SetActive(false);
        GameObject npc = Instantiate(npcPrefab, npcSpawnPoint.position, npcSpawnPoint.rotation);

        // Kamera umschalten
        if (playerCamera != null) playerCamera.gameObject.SetActive(false);
        if (cinematicCamera != null) cinematicCamera.gameObject.SetActive(true);
        if (playerMain != null) playerMain.gameObject.SetActive(false);

        DialogueManager.Instance.StartDialogue(dialogNPC);
        yield return new WaitUntil(() => !DialogueManager.Instance.IsInDialogue);

        // d) Aufräumen und zurückschalten
        if (cinematicCamera != null) cinematicCamera.gameObject.SetActive(false);
        if (playerCamera != null) playerCamera.gameObject.SetActive(true);
        if (playerMain != null) playerMain.gameObject.SetActive(true);
        if (hotbarUI != null) hotbarUI.SetActive(true);

        Destroy(npc);

        // e) Monster-Tür aktivieren / Wände umschalten wie bisher
        if (monsterDoor != null)
            monsterDoor.SetActive(true);
        if (MonsterDoorFrameWithoutcode != null)
            MonsterDoorFrameWithoutcode.SetActive(false);
        if (Invinsiblewallmidlele != null)
            Invinsiblewallmidlele.SetActive(false);
        if (Invinsiblewallafter != null)
            Invinsiblewallafter.SetActive(true);
        if (Invinsiblewallmidlele2 != null)
            Invinsiblewallmidlele2.SetActive(false);
        if (Invinsiblewallafter2 != null)
            Invinsiblewallafter2.SetActive(true);

        // f) Diese Szene nicht länger als Interactable behalten
        gameObject.layer = LayerMask.NameToLayer("Default");
    }

    private IEnumerator RotateLocal(Transform t, Quaternion from, Quaternion to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            t.localRotation = Quaternion.Slerp(from, to, elapsed / duration);
            yield return null;
        }
        t.localRotation = to;
    }
}
