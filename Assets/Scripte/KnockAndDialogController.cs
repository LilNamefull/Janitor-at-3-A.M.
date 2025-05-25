using UnityEngine;
using System.Collections;

public class KnockAndDialogController : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource knockAudio;       // 3D AudioSource am Locker

    [Header("Ranges")]
    public float knockMaxDistance = 12f; // Hörweite
    public float knockMinDistance = 2f;  // Ab hier pausiert
    public float interactDistance = 3f; // Ab hier E-Interaktion möglich

    [Header("Dialog")]
    public string[] dialogAfterOpen;     // Dialog 1 nach E-Drücken
    public string[] dialogNPC;           // Dialog 2 mit NPC
    public GameObject npcPrefab;         // NPC-Prefab
    public Transform npcSpawnPoint;      // NPC-Spawn-Position

    [Header("Timings")]
    public float dialogDelay = 0.5f;     // Warte vor Dialog 1
    public float lookSpeed = 2f;       // Drehtempo

    [Header("Camera Cut-Pivot")]
    public Transform cameraHolder;       // Leeres GameObject, Parent der MainCamera
    public float lookAngle = 45f;        // Winkel nach links
    public float rotateDuration = 0.5f;  // Dauer des Schwenks

    private Transform player;
    private bool isKnocking = false;
    private bool cutsceneStarted = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (knockAudio == null) Debug.LogError("knockAudio fehlt!");
        if (cameraHolder == null) Debug.LogError("cameraHolder fehlt!");

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
            knockAudio.Play();
            isKnocking = true;
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
                StartCoroutine(DialogSequence());
            }
        }
    }

    private IEnumerator DialogSequence()
    {
        // a) Warte vor erstem Dialog
        yield return new WaitForSecondsRealtime(dialogDelay);

        // b) Dialog 1 starten
        DialogueManager.Instance.StartDialogue(dialogAfterOpen);
        yield return new WaitUntil(() => !DialogueManager.Instance.IsInDialogue);

        // c) Speicher Original-Local-Rotation des CameraHolder
        Quaternion origLocalRot = cameraHolder.localRotation;

        // d) Ziel-Rotation 45° nach links
        Quaternion targetLocalRot = origLocalRot * Quaternion.Euler(0f, -lookAngle, 0f);

        // e) Schwenk nach links
        yield return RotateLocal(cameraHolder, origLocalRot, targetLocalRot, rotateDuration);

        // f) NPC spawnen + Dialog 2 starten
        GameObject npc = Instantiate(npcPrefab, npcSpawnPoint.position, npcSpawnPoint.rotation);
        DialogueManager.Instance.StartDialogue(dialogNPC);
        yield return new WaitUntil(() => !DialogueManager.Instance.IsInDialogue);

        // g) Rück-Schwenk zum Original
        yield return RotateLocal(cameraHolder, targetLocalRot, origLocalRot, rotateDuration);

        // h) Aufräumen
        Destroy(npc);

       
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

