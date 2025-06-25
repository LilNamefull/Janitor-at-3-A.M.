// Assets/Scripts/KnockAndDialogController.cs
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

    [Header("Player & Cameras")]
    public GameObject playerMain;        // z.B. Player-Root zum Deaktivieren
    public Camera playerCamera;
    public Camera cinematicCamera;

    [Header("Monster Door")]
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
    [Tooltip("Sekunden nach Start des Klopfens, bis automatisch der Auto-Dialog startet")]
    public float autoDialogDelay = 5f;
    [Tooltip("Dialogzeilen, die nach autoDialogDelay automatisch angezeigt werden")]
    public string[] autoDialogLines;

    private Coroutine autoDialogCoroutine = null;
    private bool autoDialogShown = false;

    private Transform player;
    private bool isKnocking = false;
    private bool cutsceneStarted = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
            Debug.LogError("[KnockAndDialogController] Player nicht gefunden (Tag 'Player').");

        // Initiale Kamera/Player-Settings
        if (playerMain != null) playerMain.SetActive(true);
        if (playerCamera != null) playerCamera.gameObject.SetActive(true);
        if (cinematicCamera != null) cinematicCamera.gameObject.SetActive(false);
        if (hotbarUI != null) hotbarUI.SetActive(true);

        // Monster-Tür initial deaktivieren
        if (monsterDoor != null) monsterDoor.SetActive(false);
        if (MonsterDoorFrameWithoutcode != null) MonsterDoorFrameWithoutcode.SetActive(true);
        if (Invinsiblewallmidlele != null) Invinsiblewallmidlele.SetActive(false);
        if (Invinsiblewallafter != null) Invinsiblewallafter.SetActive(false);
        if (Invinsiblewallmidlele2 != null) Invinsiblewallmidlele2.SetActive(false);
        if (Invinsiblewallafter2 != null) Invinsiblewallafter2.SetActive(false);

        // AudioSource konfigurieren
        if (knockAudio != null)
        {
            knockAudio.spatialBlend = 1f;
            knockAudio.loop = true;
            knockAudio.playOnAwake = false;
            knockAudio.minDistance = knockMinDistance;
            knockAudio.maxDistance = knockMaxDistance;
        }
        else
        {
            Debug.LogError("[KnockAndDialogController] knockAudio nicht gesetzt!");
        }
    }

    void Update()
    {
        if (player == null) return;

        // 1) Starte Klopfen, sobald Task "InvestigateNoise" aktiv ist
        if (!isKnocking)
        {
            if (TaskManager.Instance == null)
            {
                Debug.LogWarning("[KnockAndDialogController] TaskManager.Instance ist null; Klopfen nicht gestartet.");
            }
            else if (TaskManager.Instance.HasTask("InvestigateNoise"))
            {
                // Klopfen beginnen
                if (backgroundMusic != null && backgroundMusic.isPlaying)
                    backgroundMusic.Stop();
                if (knockAudio != null)
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

                // Wände umschalten
                if (Invinsiblewallbefor != null) Invinsiblewallbefor.SetActive(false);
                if (Invinsiblewallmidlele != null) Invinsiblewallmidlele.SetActive(true);
                if (Invinsiblewallbefor2 != null) Invinsiblewallbefor2.SetActive(false);
                if (Invinsiblewallmidlele2 != null) Invinsiblewallmidlele2.SetActive(true);
            }
        }

        if (isKnocking)
        {
            // 2) Pause/UnPause basierend auf Spieler-Abstand
            if (knockAudio == null) return;
            float dist = Vector3.Distance(player.position, knockAudio.transform.position);
            if (dist <= knockMinDistance && knockAudio.isPlaying)
                knockAudio.Pause();
            else if (dist > knockMinDistance && !knockAudio.isPlaying)
                knockAudio.UnPause();

            // 3) E-Interaktion, wenn nah genug und Cutscene noch nicht gestartet
            if (dist <= interactDistance && !cutsceneStarted && Input.GetKeyDown(KeyCode.E))
            {
                cutsceneStarted = true;
                if (autoDialogCoroutine != null)
                {
                    StopCoroutine(autoDialogCoroutine);
                    autoDialogCoroutine = null;
                }
                if (knockAudio != null) knockAudio.Stop();

                // Collider/Interactable ausschalten
                var col = GetComponent<Collider>();
                if (col != null) col.enabled = false;
                var interactable = GetComponent<Interactable>();
                if (interactable != null) interactable.enabled = false;

                // Hotbar ausblenden
                if (hotbarUI != null) hotbarUI.SetActive(false);

                StartCoroutine(DialogSequence());
            }
        }
    }

    private IEnumerator AutoTriggerDialogAfterDelay()
    {
        yield return new WaitForSecondsRealtime(autoDialogDelay);
        autoDialogCoroutine = null;

        if (isKnocking && !cutsceneStarted && !autoDialogShown)
        {
            autoDialogShown = true;
            if (knockAudio != null) knockAudio.Pause();

            // Freeze Game und Cursor freigeben
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Starte Auto-Dialog
            if (autoDialogLines != null && autoDialogLines.Length > 0)
            {
                if (hotbarUI != null) hotbarUI.SetActive(false);
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
            if (knockAudio != null) knockAudio.UnPause();
            if (hotbarUI != null) hotbarUI.SetActive(true);
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
        GameObject npc = null;
        if (npcPrefab != null && npcSpawnPoint != null)
            npc = Instantiate(npcPrefab, npcSpawnPoint.position, npcSpawnPoint.rotation);

        // Kamera umschalten
        if (playerCamera != null) playerCamera.gameObject.SetActive(false);
        if (cinematicCamera != null) cinematicCamera.gameObject.SetActive(true);
        if (playerMain != null) playerMain.SetActive(false);

        DialogueManager.Instance.StartDialogue(dialogNPC);
        yield return new WaitUntil(() => !DialogueManager.Instance.IsInDialogue);

        // d) Aufräumen und zurückschalten
        if (cinematicCamera != null) cinematicCamera.gameObject.SetActive(false);
        if (playerCamera != null) playerCamera.gameObject.SetActive(true);
        if (playerMain != null) playerMain.SetActive(true);
        if (hotbarUI != null) hotbarUI.SetActive(true);

        if (npc != null) Destroy(npc);

        // Task wechseln: InvestigateNoise entfernen, EscapeExit hinzufügen
        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.RemoveTask("InvestigateNoise");
            TaskManager.Instance.AddTask("EscapeExit", "Task: Escape through the emergency exit", "");
        }

        // e) Monster-Tür aktivieren / Wände umschalten
        if (monsterDoor != null) monsterDoor.SetActive(true);
        if (MonsterDoorFrameWithoutcode != null) MonsterDoorFrameWithoutcode.SetActive(false);
        if (Invinsiblewallmidlele != null) Invinsiblewallmidlele.SetActive(false);
        if (Invinsiblewallafter != null) Invinsiblewallafter.SetActive(true);
        if (Invinsiblewallmidlele2 != null) Invinsiblewallmidlele2.SetActive(false);
        if (Invinsiblewallafter2 != null) Invinsiblewallafter2.SetActive(true);

        // f) Interactable deaktivieren
        gameObject.layer = LayerMask.NameToLayer("Default");
    }
}
