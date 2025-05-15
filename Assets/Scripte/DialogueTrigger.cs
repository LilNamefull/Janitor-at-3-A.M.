using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialog Lines")]
    [TextArea(2, 5)]
    public string[] dialogueLines;

    [Header("Spawn on Complete")]
    public GameObject spawnPrefab;    // wird nach vollständigem Dialog instanziert
    public Transform spawnPoint;      // Position/Rotation für das neue Objekt

    [Header("Interaction")]
    public float interactDistance = 2f;

    private Transform player;
    private Camera cam;
    private Collider col;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        cam = Camera.main;
        col = GetComponent<Collider>();
    }

    void Update()
    {
        // 1) Distanzprüfung
        if (Vector3.Distance(transform.position, player.position) > interactDistance)
            return;

        // 2) Blickrichtung prüfen
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            if (hit.collider == col && Input.GetKeyDown(KeyCode.E))
            {
                // Dialog mit individuellem Spawn starten
                DialogueManager.Instance.StartDialogue(dialogueLines, spawnPrefab, spawnPoint);
            }
        }
    }
}
