using UnityEngine;
using System.Collections;

public class LockerCutsceneController : MonoBehaviour
{
    [Header("Animation (Legacy)")]
    public Animation doorAnimation;        // Animation-Komponente auf deinem LockerDoorPivot

    [Header("Audio & Trigger")]
    public AudioSource knockAudio;
    private bool inRange = false;
    private bool cutsceneStarted = false;

    [Header("Camera Points")]
    public Transform cameraCutPoint;
    public Transform cameraReturnPoint;
    public float cameraMoveTime = 1f;

    [Header("Dialog")]
    public string[] dialogAfterOpen;
    public string[] dialogNPC;
    public GameObject npcPrefab;
    public Transform npcSpawnPoint;

    [Header("Timings & Speed")]
    public float pauseAfterOpen = 0.5f;
    public float lookSpeed = 2f;

    private Transform player;
    private Transform cam;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        cam = Camera.main.transform;

       if (doorAnimation == null)
        Debug.LogError("doorAnimation fehlt!");
        else if (doorAnimation.clip == null)
        Debug.LogError("Animation.clip ist null – Default Clip nicht gesetzt!");
        else
        Debug.Log("Animation-Setup OK: Default Clip = " + doorAnimation.clip.name);
        
        if (cameraCutPoint == null) Debug.LogError("cameraCutPoint fehlt!");
        if (cameraReturnPoint == null) Debug.LogError("cameraReturnPoint fehlt!");

        // Stelle sicher, dass der Clip nur einmal läuft:
        doorAnimation.wrapMode = WrapMode.Once;
        doorAnimation.clip.wrapMode = WrapMode.Once;
    }

    void Update()
    {
        if (inRange && GameManagerIntro.Instance.allTasksDone && !cutsceneStarted && Input.GetKeyDown(KeyCode.E))
        {
            cutsceneStarted = true;
            if (knockAudio != null && knockAudio.isPlaying) knockAudio.Stop();
            StartCoroutine(CutsceneSequence());
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inRange = true;
            if (GameManagerIntro.Instance.allTasksDone && knockAudio != null)
                knockAudio.Play();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            inRange = false;
    }

    /// Wird vom GameManagerIntro aufgerufen
    public void PlayKnock()
    {
        if (knockAudio != null && !knockAudio.isPlaying)
            knockAudio.Play();
    }

    private IEnumerator CutsceneSequence()
    {
        // 1) Freeze & save camera
        Vector3 origPos = cam.position;
        Quaternion origRot = cam.rotation;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 2) Move camera to locker
        yield return MoveCamera(origPos, origRot, cameraCutPoint.position, cameraCutPoint.rotation);

        // 3) Play the default door animation
        if (doorAnimation != null && doorAnimation.clip != null)
        {
            doorAnimation.Play();
            float len = doorAnimation.clip.length;
            yield return new WaitForSecondsRealtime(len + pauseAfterOpen);
        }
        else
        {
            Debug.LogError("doorAnimation.clip ist null – bitte im Inspector den Default-Clip setzen!");
            yield return new WaitForSecondsRealtime(pauseAfterOpen);
        }

        // 4) Dialog 1
        if (dialogAfterOpen != null && dialogAfterOpen.Length > 0)
            DialogueManager.Instance.StartDialogue(dialogAfterOpen);
        while (DialogueManager.Instance.IsInDialogue)
            yield return null;

        // 5) Rotate to NPC
        yield return RotatePlayerTo(npcSpawnPoint.position);

        // 6) Spawn NPC & Dialog 2
        GameObject npc = null;
        if (npcPrefab != null && npcSpawnPoint != null)
            npc = Instantiate(npcPrefab, npcSpawnPoint.position, npcSpawnPoint.rotation);
        if (dialogNPC != null && dialogNPC.Length > 0)
            DialogueManager.Instance.StartDialogue(dialogNPC);
        while (DialogueManager.Instance.IsInDialogue)
            yield return null;

        // 7) Rotate back to locker
        yield return RotatePlayerTo(cameraReturnPoint.position);

        // 8) Destroy NPC
        if (npc != null) Destroy(npc);

        // 9) Unfreeze
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private IEnumerator MoveCamera(Vector3 fromP, Quaternion fromR, Vector3 toP, Quaternion toR)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / cameraMoveTime;
            cam.position = Vector3.Lerp(fromP, toP, t);
            cam.rotation = Quaternion.Slerp(fromR, toR, t);
            yield return null;
        }
    }

    private IEnumerator RotatePlayerTo(Vector3 target)
    {
        Vector3 dir = (new Vector3(target.x, player.position.y, target.z) - player.position).normalized;
        Quaternion start = player.rotation;
        Quaternion end = Quaternion.LookRotation(dir);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * lookSpeed;
            player.rotation = Quaternion.Slerp(start, end, t);
            yield return null;
        }
        player.rotation = end;
    }
}
