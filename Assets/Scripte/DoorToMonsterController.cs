using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class DoorToMonsterController : MonoBehaviour
{
    [Header("Interaction")]
    public float interactDistance = 3f;
    public string promptMessage = "Drücke E, um die Tür zu öffnen";
    public TextMeshProUGUI promptText;            // UI-Text für “Drücke E…“

    [Header("Door")]
    public Door door;                  // Dein Tür-Script
    public float doorOpenTime = 1f;     // Dauer, bis die Tür offen ist

    [Header("Camera Pivot")]
    public Transform cameraHolder;       // Leer-GameObject über der MainCamera
    public float rotateAngle = 180f;   // Schwenkwinkel
    public float rotateDuration = 0.5f;   // Dauer des Schwenks

    [Header("Dialogues")]
    public float dialogDelay = 0.5f;       // Warte vor Dialog1
    public string[] dialog1;                  // Dialog nach Türöffnung
    public string[] dialog2;                  // Zweiter Dialog nach Schwenk

    [Header("Scene")]
    public string chaseSceneName = "ChaseScene";

    private Transform playerCam;
    private bool sequenceStarted = false;

    void Start()
    {
        playerCam = Camera.main.transform;

        if (promptText != null)
            promptText.gameObject.SetActive(false);

        if (door == null)
            Debug.LogError("Bitte Door-Referenz setzen!");

        // Collider muss kein Trigger sein
        GetComponent<Collider>().isTrigger = false;
    }

    void Update()
    {
        if (sequenceStarted) return;

        // 1) Raycast aus Kamera
        Ray ray = new Ray(playerCam.position, playerCam.forward);
        if (Physics.Raycast(ray, out var hit, interactDistance))
        {
            if (hit.collider == GetComponent<Collider>())
            {
                // Prompt einblenden
                if (promptText != null)
                {
                    promptText.text = promptMessage;
                    promptText.gameObject.SetActive(true);
                }

                // 2) Interaktion
                if (Input.GetKeyDown(KeyCode.E))
                {
                    sequenceStarted = true;
                    if (promptText != null) promptText.gameObject.SetActive(false);
                    StartCoroutine(CutsceneSequence());
                }
                return;
            }
        }

        // Prompt ausblenden, wenn nicht mehr in Range
        if (promptText != null && promptText.gameObject.activeSelf)
            promptText.gameObject.SetActive(false);
    }

    private IEnumerator CutsceneSequence()
    {
        // 1) Tür öffnen
        door.Interact();
        yield return new WaitForSecondsRealtime(doorOpenTime);

        // 2) Kurzes Delay
        yield return new WaitForSecondsRealtime(dialogDelay);

        // 3) Dialog 1
        DialogueManager.Instance.StartDialogue(dialog1);
        yield return new WaitUntil(() => !DialogueManager.Instance.IsInDialogue);

        // 4) Kamera um rotateAngle schwenken (lokal!)
        Quaternion origRot = cameraHolder.localRotation;
        Quaternion targetRot = origRot * Quaternion.Euler(0f, rotateAngle, 0f);
        yield return RotateLocal(cameraHolder, origRot, targetRot, rotateDuration);

        // 5) Dialog 2
        DialogueManager.Instance.StartDialogue(dialog2);
        yield return new WaitUntil(() => !DialogueManager.Instance.IsInDialogue);

        // 6) Szene wechseln
        UnityEngine.SceneManagement.SceneManager.LoadScene("ChaseScene");
        yield break;
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
