using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI Elements")]
    public GameObject dialogPanel;             // Das Panel am unteren Bildschirmrand
    public TextMeshProUGUI dialogText;         // Textfeld im Panel
    public Button nextButton;                  // Weiter‐Button
    public Button exitButton;                  // Abbrechen‐Button

    private string[] lines;                    // Alle Zeilen des aktuellen Dialogs
    private int currentLine;                   // Index der gerade angezeigten Zeile
    private bool inDialogue;                   // Steuert, ob wir aktuell in einem Dialog sind
    private bool aborted;                      // Markiert, ob der Dialog durch „Exit“ abgebrochen wurde

    public GameObject Interaction;

    /// <summary>
    /// Gibt zurück, ob gerade ein Dialog aktiv ist.
    /// </summary>
    public bool IsInDialogue => inDialogue;

    /// <summary>
    /// Wird nur dann FEUERN, wenn der Dialog bis zur letzten Zeile durchgeklickt wurde (nicht bei Exit).
    /// </summary>
    public event Action OnDialogueComplete;

    /// <summary>
    /// Wird nur dann FEUERN, wenn der Dialog vorzeitig (z. B. über den Exit‐Button) beendet wurde.
    /// </summary>
    public event Action OnDialogueAborted;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        dialogPanel.SetActive(false);

        nextButton.onClick.AddListener(OnNextButton);
        exitButton.onClick.AddListener(OnExitButton);
    }
    void Start()
    {
        if (Interaction != null) Interaction.gameObject.SetActive(true);
    }
    /// <summary>
    /// Startet einen Dialog mit den übergebenen Zeilen. 
    /// Neue Spawns werden nicht mehr hier angesprochen, sondern über OnDialogueComplete.
    /// </summary>
    public void StartDialogue(string[] dialogueLines)
    {
        if (Interaction != null) Interaction.gameObject.SetActive(false);
        if (inDialogue) return;

        lines = dialogueLines;
        currentLine = 0;
        inDialogue = true;
        aborted = false;

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
        {
            // Noch weitere Zeilen vorhanden → nächste anzeigen
            ShowLine();
        }
        else
        {
            // Letzte Zeile war gerade gezeigt → Dialog beenden
            EndDialogue();
            // FEUERE OnDialogueComplete nur, wenn nicht abgebrochen
            if (!aborted)
                OnDialogueComplete?.Invoke();
        }
    }

    private void OnExitButton()
    {
        if (!inDialogue) return;

        // Dialog vorzeitig abbrechen
        aborted = true;
        EndDialogue();
        OnDialogueAborted?.Invoke();
    }

    private void EndDialogue()
    {
        inDialogue = false;
        dialogPanel.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Aufräumen: Zeilen‐Array löschen, Zeiger zurücksetzen
        lines = null;
        currentLine = 0;

        if (Interaction != null) Interaction.gameObject.SetActive(true);
    }
}
