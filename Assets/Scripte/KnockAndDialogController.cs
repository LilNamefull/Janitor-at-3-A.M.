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

    [Header("Camera Cut-Pivot")]
    public Transform cameraHolder;       // Leeres GameObject, Parent der MainCamera
    public float lookAngle = 45f;        // Winkel nach links
    public float rotateDuration = 0.5f;  // Dauer des Schwenks

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
    private Transform player;
    private bool isKnocking = false;
    private bool cutsceneStarted = false;

    public GameObject hotbarUI;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (knockAudio == null) Debug.LogError("knockAudio fehlt!");
        if (cameraHolder == null) Debug.LogError("cameraHolder fehlt!");

        // Monster-Tür initial deaktivieren
        if (monsterDoor != null)
            monsterDoor.SetActive(false);
        if (MonsterDoorFrameWithoutcode != null)
            MonsterDoorFrameWithoutcode.SetActive(true);
        if (Invinsiblewallmidlele != null)
            Invinsiblewallmidlele.SetActive(false);
        if (Invinsiblewallafter != null)
            Invinsiblewallafter.SetActive(false );
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


        // 1) Starte Klopfen, sobald alle Aufgaben erledigt sind
        if (!isKnocking && GameManagerIntro.Instance.allTasksDone)
        {
            if (backgroundMusic != null && backgroundMusic.isPlaying)
                backgroundMusic.Stop();
            knockAudio.Play();
            isKnocking = true;
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

    private IEnumerator DialogSequence()
    {
        // a) Warte vor erstem Dialog
        yield return new WaitForSecondsRealtime(dialogDelay);

        // b) Dialog 1 starten
        DialogueManager.Instance.exitButton.gameObject.SetActive(false);
        DialogueManager.Instance.StartDialogue(dialogAfterOpen);
        yield return new WaitUntil(() => !DialogueManager.Instance.IsInDialogue);

        // c) Speicher Original-Local-Rotation des CameraHolder
        Quaternion origLocalRot = cameraHolder.localRotation;

        // d) Ziel-Rotation 45° nach links
        Quaternion targetLocalRot = origLocalRot * Quaternion.Euler(0f, -lookAngle, 0f);

        // e) Schwenk nach links
        yield return RotateLocal(cameraHolder, origLocalRot, targetLocalRot, rotateDuration);

        // f) NPC spawnen + Dialog 2 starten
        DialogueManager.Instance.exitButton.gameObject.SetActive(false);
        GameObject npc = Instantiate(npcPrefab, npcSpawnPoint.position, npcSpawnPoint.rotation);
        DialogueManager.Instance.StartDialogue(dialogNPC);
        yield return new WaitUntil(() => !DialogueManager.Instance.IsInDialogue);

        // g) Rück-Schwenk zum Original
        yield return RotateLocal(cameraHolder, targetLocalRot, origLocalRot, rotateDuration);

        // h) Aufräumen
        Destroy(npc);

        // i) Monster-Tür jetzt aktivieren
        if (monsterDoor != null)
            monsterDoor.SetActive(true);
        if (MonsterDoorFrameWithoutcode !=null)
            MonsterDoorFrameWithoutcode.SetActive(false);
        if (Invinsiblewallmidlele != null)
            Invinsiblewallmidlele.SetActive(false);
        if (Invinsiblewallafter != null)
            Invinsiblewallafter.SetActive(true);
        if (Invinsiblewallmidlele2 != null)
            Invinsiblewallmidlele2.SetActive(false);
        if (Invinsiblewallafter2 != null)
            Invinsiblewallafter2.SetActive(true);

        // j) Diese Szene nicht länger als Interactable behalten
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
