using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class DialogueTriggerMultiTask : MonoBehaviour
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

    [Header("Task Integration: Hinzufügen nach Abschluss")]
    [Tooltip("IDs der Tasks, die nach vollständigem Dialog hinzugefügt/aktualisiert werden")]
    public string[] taskIDsToAdd;
    [Tooltip("Titeltexte der Tasks. Länge sollte >= taskIDsToAdd.Length sein")]
    public string[] taskTitlesToAdd;
    [Tooltip("Untertiteltexte der Tasks. Länge sollte >= taskIDsToAdd.Length sein")]
    public string[] taskSubtitlesToAdd;

    [Header("Task Integration: Entfernen nach Abschluss")]
    [Tooltip("IDs der Tasks, die nach vollständigem Dialog entfernt werden sollen")]
    public string[] taskIDsToRemove;

    private Transform player;
    private Camera cam;
    private Collider col;

    // Verhindert mehrfaches Feuern, solange Dialog läuft
    private bool triggered = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
            Debug.LogError("[DialogueTriggerMultiTask] Spieler nicht gefunden (Tag 'Player'?).");

        cam = Camera.main;
        if (cam == null)
            Debug.LogError("[DialogueTriggerMultiTask] Hauptkamera nicht gefunden.");

        col = GetComponent<Collider>();
        if (col == null)
            Debug.LogError("[DialogueTriggerMultiTask] Collider nicht gefunden.");
        else
            col.isTrigger = false; // Raycast-Interaktion
    }

    void Update()
    {
        if (triggered) return;

        if (player == null || cam == null || col == null) return;

        // 1) Distanz prüfen
        if (Vector3.Distance(transform.position, player.position) > interactDistance)
            return;

        // 2) Blickrichtung prüfen
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            if (hit.collider == col && Input.GetKeyDown(KeyCode.E))
            {
                // Start Dialog
                triggered = true;

                // Event-Handler abonnieren
                DialogueManager.Instance.OnDialogueComplete += HandleDialogueComplete;
                DialogueManager.Instance.OnDialogueAborted += HandleDialogueAborted;

                DialogueManager.Instance.StartDialogue(dialogueLines);
            }
        }
    }

    private void HandleDialogueComplete()
    {
        // 1) Sofort abmelden
        DialogueManager.Instance.OnDialogueComplete -= HandleDialogueComplete;
        DialogueManager.Instance.OnDialogueAborted -= HandleDialogueAborted;

        // 2) Spawns
        if (spawnPrefab1 != null && spawnPoint1 != null)
            Instantiate(spawnPrefab1, spawnPoint1.position, spawnPoint1.rotation);
        if (spawnPrefab2 != null && spawnPoint2 != null)
            Instantiate(spawnPrefab2, spawnPoint2.position, spawnPoint2.rotation);

        // 3) Tasks entfernen
        if (taskIDsToRemove != null)
        {
            foreach (var removeID in taskIDsToRemove)
            {
                if (!string.IsNullOrEmpty(removeID))
                {
                    TaskUIManager.Instance?.RemoveTask(removeID);
                }
            }
        }

        // 4) Tasks hinzufügen/aktualisieren
        if (taskIDsToAdd != null)
        {
            int count = taskIDsToAdd.Length;
            for (int i = 0; i < count; i++)
            {
                string id = taskIDsToAdd[i];
                if (string.IsNullOrEmpty(id)) continue;

                string title = "";
                if (taskTitlesToAdd != null && i < taskTitlesToAdd.Length)
                    title = taskTitlesToAdd[i];
                string subtitle = "";
                if (taskSubtitlesToAdd != null && i < taskSubtitlesToAdd.Length)
                    subtitle = taskSubtitlesToAdd[i];

                TaskUIManager.Instance?.UpdateTask(id, title, subtitle);
            }
        }

        // 5) Reset triggered, damit später erneut möglich (z.B. NPC erneut ansprechbar)
        triggered = false;
    }

    private void HandleDialogueAborted()
    {
        DialogueManager.Instance.OnDialogueComplete -= HandleDialogueComplete;
        DialogueManager.Instance.OnDialogueAborted -= HandleDialogueAborted;
        triggered = false;
    }

    void OnDestroy()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueComplete -= HandleDialogueComplete;
            DialogueManager.Instance.OnDialogueAborted -= HandleDialogueAborted;
        }
    }
}
