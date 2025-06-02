using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialog Lines")]
    [TextArea(2, 5)]
    public string[] dialogueLines;

    [Header("Spawn on Complete (1)")]
    public GameObject spawnPrefab1;
    public Transform spawnPoint1;

    [Header("Spawn on Complete (2)")]
    public GameObject spawnPrefab2;
    public Transform spawnPoint2;

    [Header("Interaction")]
    public float interactDistance = 2f;

    private Transform player;
    private Camera cam;
    private Collider col;

    // Verhindert, dass der Trigger neu feuert, solange ein Dialog läuft
    private bool triggered = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        cam = Camera.main;
        col = GetComponent<Collider>();
        col.isTrigger = false; // Wir nutzen Raycast, kein Trigger
    }

    void Update()
    {
        // Wenn wir bereits einen Dialog gestartet haben und auf dessen Abschluss warten, tun wir nichts
        if (triggered) return;

        // 1) Distanz zum Spieler prüfen
        if (Vector3.Distance(transform.position, player.position) > interactDistance)
            return;

        // 2) Blickrichtung checken: Raycast aus Bildschirmmitte
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            if (hit.collider == col && Input.GetKeyDown(KeyCode.E))
            {
                // Dialog starten und uns selbst als „aktiver Trigger“ markieren
                triggered = true;
                // 1) Nur JETZT abonnieren, damit nur diese Instanz die Completion/Abort‐Events empfängt
                DialogueManager.Instance.OnDialogueComplete += HandleDialogueComplete;
                DialogueManager.Instance.OnDialogueAborted += HandleDialogueAborted;

                // 2) Dialog wirklich starten
                DialogueManager.Instance.StartDialogue(dialogueLines);
            }
        }
    }

    private void HandleDialogueComplete()
    {
        // 1) Sofort wieder abmelden, damit wir nicht erneut reagieren
        DialogueManager.Instance.OnDialogueComplete -= HandleDialogueComplete;
        DialogueManager.Instance.OnDialogueAborted -= HandleDialogueAborted;

        // 2) Spawne nur die Objekte, die für „Complete“ vorgesehen sind
        if (spawnPrefab1 != null && spawnPoint1 != null)
            Instantiate(spawnPrefab1, spawnPoint1.position, spawnPoint1.rotation);

        if (spawnPrefab2 != null && spawnPoint2 != null)
            Instantiate(spawnPrefab2, spawnPoint2.position, spawnPoint2.rotation);

        // 3) Zurücksetzen, um nächsten Dialog erneut möglich zu machen
        triggered = false;
    }

    private void HandleDialogueAborted()
    {
        // 1) Sofort wieder abmelden, keine Spawns
        DialogueManager.Instance.OnDialogueComplete -= HandleDialogueComplete;
        DialogueManager.Instance.OnDialogueAborted -= HandleDialogueAborted;

        // 2) Trigger zurücksetzen, damit der Spieler später erneut E drücken kann
        triggered = false;
    }

    void OnDestroy()
    {
        // Falls dieses Objekt zerstört wird, sollten wir auf jeden Fall abmelden
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueComplete -= HandleDialogueComplete;
            DialogueManager.Instance.OnDialogueAborted -= HandleDialogueAborted;
        }
    }
}

