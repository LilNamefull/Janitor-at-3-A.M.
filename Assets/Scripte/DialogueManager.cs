using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI Elements")]
    public GameObject dialogPanel;  // Dein Panel unten am Bildschirm
    public TextMeshProUGUI dialogText;         // Der Text im Panel
    public Button nextButton;
    public Button exitButton;

    // interne Speicher für das zu spawnende Objekt
    private GameObject pendingSpawnPrefab;
    private Transform pendingSpawnPoint;

    private string[] lines;
    private int currentLine;
    private bool inDialogue;
    private bool aborted;

    public bool IsInDialogue
    {
        get { return inDialogue; }
    }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        dialogPanel.SetActive(false);
        nextButton.onClick.AddListener(OnNextButton);
        exitButton.onClick.AddListener(OnExitButton);
    }

    /// <summary>
    /// Standard-Start ohne Spawn
    /// </summary>
    public void StartDialogue(string[] dialogueLines)
    {
        StartDialogue(dialogueLines, null, null);
    }

    /// <summary>
    /// Startet Dialog und merkt sich Prefab+Point fürs Ende
    /// </summary>
    public void StartDialogue(string[] dialogueLines, GameObject spawnPrefab, Transform spawnPoint)
    {
        if (inDialogue) return;

        lines = dialogueLines;
        currentLine = 0;
        inDialogue = true;
        aborted = false;

        pendingSpawnPrefab = spawnPrefab;
        pendingSpawnPoint = spawnPoint;

        dialogPanel.SetActive(true);
        ShowLine();

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ShowLine()
    {
        dialogText.text = lines[currentLine];
    }

    private void OnNextButton()
    {
        if (!inDialogue) return;

        currentLine++;
        if (currentLine < lines.Length)
            ShowLine();
        else
            EndDialogue();
    }

    private void OnExitButton()
    {
        if (!inDialogue) return;

        aborted = true;
        EndDialogue();
    }

    private void EndDialogue()
    {
        inDialogue = false;
        dialogPanel.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Nur wenn nicht abgebrochen und ein Prefab übergeben wurde
        if (!aborted && pendingSpawnPrefab != null && pendingSpawnPoint != null)
        {
            Instantiate(pendingSpawnPrefab, pendingSpawnPoint.position, pendingSpawnPoint.rotation);
        }

        // aufräumen
        pendingSpawnPrefab = null;
        pendingSpawnPoint = null;
    }
}