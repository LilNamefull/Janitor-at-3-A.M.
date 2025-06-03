using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class FinalSequenceController : MonoBehaviour
{
    [Header("Cameras")]
    [Tooltip("Die MainCamera des Spielers, die während der Sequenz deaktiviert wird.")]
    public Camera playerCamera;
    [Tooltip("Die feste Ende-Kamera, die den NPC durchgehend anvisiert.")]
    public Camera cinematicCamera;

    [Header("Hotbar")]
    [Tooltip("Das GameObject, das die gesamte Hotbar enthält. Wird während Dialog+Comic deaktiviert.")]
    public GameObject hotbarUI;

    [Header("Nach Dialog 1: NPC spawnen")]
    public GameObject npcAfterDialog1Prefab;
    public Transform npcAfterDialog1SpawnPoint;

    [Header("Dialog-Sequenzen")]
    public string[] dialogLines1;
    public string[] dialogLines2;
    public string[] dialogLines3;

    [Header("Comic-Sequenz (weiß überdeckt)")]
    public ComicController comicController;

    [Header("Final-Entscheidung nach Dialog 3")]
    [Tooltip("Panel mit den beiden Buttons: Refuse und Agree.")]
    public GameObject finalDecisionPanel;
    public Button refuseButton;
    public Button agreeButton;

    [Header("Final-Outcome (schwarzer Hintergrund + Bild)")]
    [Tooltip("Panel, das am Ende das Resultat-Bild anzeigt.")]
    public GameObject finalOutcomePanel;
    [Tooltip("Image-Komponente innerhalb des FinalOutcomePanel.")]
    public Image finalOutcomeImage;
    [Tooltip("Sprite, das bei Refuse angezeigt wird.")]
    public Sprite refuseSprite;
    [Tooltip("Sprite, das bei Agree angezeigt wird.")]
    public Sprite agreeSprite;
    [Tooltip("Button, der 'Ende' anzeigt und zur Credits-Szene führt.")]
    public Button endButton;
    [Tooltip("Name der Szene mit den Credits.")]
    public string creditsSceneName = "Credits";

    [Header("Interaction")]
    [Tooltip("Abstand, ab dem der Spieler mit 'E' interagieren kann.")]
    public float interactDistance = 2f;

    public AudioSource backgroundMusic;
    public AudioSource comicMusic;


    // Privates
    private Transform player;
    private Camera cam;
    private Collider col;
    private bool triggered = false;
    private bool abortedThisDialog = false;

    void Start()
    {
        

        player = GameObject.FindGameObjectWithTag("Player").transform;
        cam = Camera.main;
        col = GetComponent<Collider>();
        col.isTrigger = false;

        // CinematicCamera deaktivieren, PlayerCamera aktivieren
        if (cinematicCamera != null) cinematicCamera.gameObject.SetActive(false);
        if (playerCamera != null) playerCamera.gameObject.SetActive(true);

        // Alle Panels deaktivieren
        if (comicController != null) comicController.gameObject.SetActive(false);
        if (finalDecisionPanel != null) finalDecisionPanel.SetActive(false);
        if (finalOutcomePanel != null) finalOutcomePanel.SetActive(false);

        // Hotbar am Anfang aktiv
        if (hotbarUI != null) hotbarUI.SetActive(true);
    }

    void Update()
    {
        if (triggered) return;

        // 1) Abstand prüfen
        if (Vector3.Distance(transform.position, player.position) > interactDistance)
            return;

        // 2) Blickrichtung prüfen per Raycast aus Bildschirmmitte
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            if (hit.collider == col && Input.GetKeyDown(KeyCode.E))
            {
                triggered = true;
                StartCoroutine(RunFinalSequence());
            }
        }
    }

    private IEnumerator RunFinalSequence()
    {
        // ==== 0) Hotbar deaktivieren ====
        if (hotbarUI != null)
            hotbarUI.SetActive(false);


        // ==== 1) Dialog 1 abspielen ====
        DialogueManager.Instance.exitButton.gameObject.SetActive(false);

        abortedThisDialog = false;
        DialogueManager.Instance.OnDialogueComplete += OnDialog1Complete;
        DialogueManager.Instance.OnDialogueAborted += OnDialog1Aborted;
        DialogueManager.Instance.StartDialogue(dialogLines1);
        yield return new WaitUntil(() => !DialogueManager.Instance.IsInDialogue);
        DialogueManager.Instance.OnDialogueComplete -= OnDialog1Complete;
        DialogueManager.Instance.OnDialogueAborted -= OnDialog1Aborted;

        if (abortedThisDialog)
        {
            // Bei Abbruch: Hotbar wieder aktivieren und Sequenz beenden
            if (hotbarUI != null) hotbarUI.SetActive(true);
            yield break;
        }

        // ==== 2) NPC nach Dialog 1 spawnen ====
        if (npcAfterDialog1Prefab != null && npcAfterDialog1SpawnPoint != null)
        {
            Instantiate(
                npcAfterDialog1Prefab,
                npcAfterDialog1SpawnPoint.position,
                npcAfterDialog1SpawnPoint.rotation);
        }

        // ==== 3) CinematicCamera aktivieren, PlayerCamera deaktivieren ====
        if (playerCamera != null) playerCamera.gameObject.SetActive(false);
        if (cinematicCamera != null) cinematicCamera.gameObject.SetActive(true);

        // ==== 4) Dialog 2 abspielen (über CinematicCamera) ====
        DialogueManager.Instance.exitButton.gameObject.SetActive(false);
        abortedThisDialog = false;
        DialogueManager.Instance.OnDialogueComplete += OnDialog2Complete;
        DialogueManager.Instance.OnDialogueAborted += OnDialog2Aborted;
        DialogueManager.Instance.StartDialogue(dialogLines2);
        yield return new WaitUntil(() => !DialogueManager.Instance.IsInDialogue);
        DialogueManager.Instance.OnDialogueComplete -= OnDialog2Complete;
        DialogueManager.Instance.OnDialogueAborted -= OnDialog2Aborted;

        if (abortedThisDialog)
        {
            // Dialog 2 abgebrochen: Kamera umschalten, Hotbar wieder an, Sequenz beenden
            if (cinematicCamera != null) cinematicCamera.gameObject.SetActive(false);
            if (playerCamera != null) playerCamera.gameObject.SetActive(true);
            if (hotbarUI != null) hotbarUI.SetActive(true);
            yield break;
        }

        // ==== 5) Comic-Sequenz anzeigen (weiß überdeckt) ====
        // Cursor freigeben, damit der „Weiter“-Button klickbar ist
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (comicController != null)
        {
            if (backgroundMusic != null && backgroundMusic.isPlaying)
                backgroundMusic.Stop();
            if (comicMusic != null)
                comicMusic.Play();


            comicController.StartComic();
            // Warte, bis der ComicController sein Panel deaktiviert
            while (comicController.gameObject.activeSelf)
                yield return null;
        }

        // ==== 6) Dialog 3 abspielen ====
        if (comicMusic != null && comicMusic.isPlaying)
            comicMusic.Stop();
        if (backgroundMusic !=null)
           backgroundMusic.Play();
        DialogueManager.Instance.exitButton.gameObject.SetActive(false);
        abortedThisDialog = false;
        DialogueManager.Instance.OnDialogueComplete += OnDialog3Complete;
        DialogueManager.Instance.OnDialogueAborted += OnDialog3Aborted;
        DialogueManager.Instance.StartDialogue(dialogLines3);
        yield return new WaitUntil(() => !DialogueManager.Instance.IsInDialogue);
        DialogueManager.Instance.OnDialogueComplete -= OnDialog3Complete;
        DialogueManager.Instance.OnDialogueAborted -= OnDialog3Aborted;

        if (abortedThisDialog)
        {
            // Dialog 3 abgebrochen: Kamera und Hotbar zurücksetzen, Sequenz beenden
            if (cinematicCamera != null) cinematicCamera.gameObject.SetActive(false);
            if (playerCamera != null) playerCamera.gameObject.SetActive(true);
            if (hotbarUI != null) hotbarUI.SetActive(true);
            yield break;
        }

        // ==== 7) Final-Entscheidung Panel anzeigen (Refuse / Agree) ====
        if (finalDecisionPanel != null && refuseButton != null && agreeButton != null)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            finalDecisionPanel.SetActive(true);
            bool decisionMade = false;

            refuseButton.onClick.AddListener(() =>
            {
                decisionMade = true;
                ShowFinalOutcome(false); // false = Refuse
            });
            agreeButton.onClick.AddListener(() =>
            {
                decisionMade = true;
                ShowFinalOutcome(true); // true = Agree
            });

            while (!decisionMade)
                yield return null;

            refuseButton.onClick.RemoveAllListeners();
            agreeButton.onClick.RemoveAllListeners();
            finalDecisionPanel.SetActive(false);
        }

        // ==== 8) End-Button listener (Credits laden) ====
        if (endButton != null)
        {
            endButton.onClick.RemoveAllListeners();
            endButton.onClick.AddListener(() =>
            {
                SceneManager.LoadScene(creditsSceneName);
            });
        }

        // Sequenz endet hier; CinematicCamera bleibt aktiv bis Szenenwechsel
        yield break;
    }

    #region Dialog1 Handler
    private void OnDialog1Complete() { abortedThisDialog = false; }
    private void OnDialog1Aborted() { abortedThisDialog = true; }
    #endregion

    #region Dialog2 Handler
    private void OnDialog2Complete() { abortedThisDialog = false; }
    private void OnDialog2Aborted() { abortedThisDialog = true; }
    #endregion

    #region Dialog3 Handler
    private void OnDialog3Complete() { abortedThisDialog = false; }
    private void OnDialog3Aborted() { abortedThisDialog = true; }
    #endregion

    /// <summary>
    /// Blendet das FinalOutcomePanel ein und zeigt entweder refuseSprite oder agreeSprite.
    /// </summary>
    private void ShowFinalOutcome(bool agreed)
    {
        if (finalOutcomePanel == null || finalOutcomeImage == null)
        {
            Debug.LogWarning("[FinalSequence] FinalOutcomePanel oder FinalOutcomeImage fehlt!");
            return;
        }

        finalOutcomeImage.sprite = agreed ? agreeSprite : refuseSprite;
        finalOutcomePanel.SetActive(true);
    }
}
