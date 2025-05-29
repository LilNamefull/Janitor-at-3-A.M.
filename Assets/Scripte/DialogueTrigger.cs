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
    private bool triggered = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        cam = Camera.main;
        col = GetComponent<Collider>();
        col.isTrigger = false; // wir nutzen Raycast, kein Trigger
    }

    void Update()
    {
        if (triggered) return;

        // 1) Distanz prüfen
        if (Vector3.Distance(transform.position, player.position) > interactDistance)
            return;

        // 2) Blickrichtung prüfen
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            if (hit.collider == col && Input.GetKeyDown(KeyCode.E))
            {
                triggered = true; // Nur einmal
                // Dialog starten
                DialogueManager.Instance.StartDialogue(dialogueLines);
                // Coroutine starten, die auf Ende wartet und dann spawnt
                StartCoroutine(SpawnAfterDialogue());
            }
        }
    }

    private IEnumerator SpawnAfterDialogue()
    {
        // Warte bis der Dialog wirklich beendet ist
        yield return new WaitUntil(() => !DialogueManager.Instance.IsInDialogue);

        // 1) Erstes Objekt
        if (spawnPrefab1 != null && spawnPoint1 != null)
        {
            Instantiate(spawnPrefab1, spawnPoint1.position, spawnPoint1.rotation);
        }

        // 2) Zweites Objekt
        if (spawnPrefab2 != null && spawnPoint2 != null)
        {
            Instantiate(spawnPrefab2, spawnPoint2.position, spawnPoint2.rotation);
        }
    }
}
