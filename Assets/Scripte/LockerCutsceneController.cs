using UnityEngine;
using System.Collections;

public class LockerCutsceneController : MonoBehaviour
{
    [Header("References")]
    public Animator lockerAnimator;
    public AudioSource knockAudio;
    public Transform cameraCutPoint;
    public Transform cameraReturnPoint;
    public GameObject npcPrefab;
    public Transform npcSpawnPoint;
    public float cameraMoveTime = 1f;

    [Header("Dialog Lines")]
    public string[] linesAfterOpen;   // Dialog direkt nach Tür-Öffnen
    public string[] linesNPC;         // Dialog mit der NPC
    public float lookSpeed = 2f;      // Geschwindigkeit des Char-Rotates
    public float pauseAfterReturn = 0.5f;

    private bool inRange = false;
    private bool allDone => GameManagerIntro.Instance.allTasksDone;
    private bool started = false;

    private Transform player;
    private Transform cam;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        cam = Camera.main.transform;
        if (!lockerAnimator) Debug.LogError("Animator fehlt!");
    }

    void Update()
    {
        if (inRange && allDone && !started && Input.GetKeyDown(KeyCode.E))
        {
            started = true;
            StartCoroutine(Sequence());
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) inRange = true;
    }
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) inRange = false;
    }

    private IEnumerator Sequence()
    {
        // 1) Tür auf
        lockerAnimator.SetTrigger("Open");
        // Warte Animationslänge in Echtzeit
        float len = lockerAnimator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSecondsRealtime(len);

        // 2) Dialog 1
        if (linesAfterOpen != null && linesAfterOpen.Length > 0)
            DialogueManager.Instance.StartDialogue(linesAfterOpen);
        while (DialogueManager.Instance.IsInDialogue)
            yield return null;

        // 3) Charakter dreht auf NPC
        yield return RotatePlayerTo(npcSpawnPoint.position);

        // 4) NPC spawnen + Dialog 2
        GameObject npc = Instantiate(npcPrefab, npcSpawnPoint.position, npcSpawnPoint.rotation);
        if (linesNPC != null && linesNPC.Length > 0)
            DialogueManager.Instance.StartDialogue(linesNPC);
        while (DialogueManager.Instance.IsInDialogue)
            yield return null;

        // 5) Character dreht zurück zum Locker
        yield return RotatePlayerTo(cameraReturnPoint.position);

        // 6) kurze Pause
        yield return new WaitForSecondsRealtime(pauseAfterReturn);

        // 7) NPC entfernen & Cutscene Ende
        Destroy(npc);

        // Spiel freigeben (falls du Time.timeScale verwendest)
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// Dreht Player langsam so, dass er auf targetPos schaut.
    /// </summary>
    private IEnumerator RotatePlayerTo(Vector3 targetPos)
    {
        Vector3 dir = (new Vector3(targetPos.x, player.position.y, targetPos.z) - player.position).normalized;
        if (dir.sqrMagnitude < 0.001f) yield break;

        Quaternion from = player.rotation;
        Quaternion to = Quaternion.LookRotation(dir, Vector3.up);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * lookSpeed;
            player.rotation = Quaternion.Slerp(from, to, t);
            yield return null;
        }
        player.rotation = to;
    }
}
