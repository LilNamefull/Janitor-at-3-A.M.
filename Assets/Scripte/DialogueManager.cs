using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI Elements")]
    public GameObject dialogPanel;       // Panel unten am Bildschirmrand
    public TextMeshProUGUI dialogText;   // Der eigentliche Dialog-Text
    public Button nextButton;
    public Button exitButton;

    [Header("Portrait")]
    public Image portraitImage;          // Image-UI-Element für das Portrait
    public Sprite defaultPortrait;       // Default, wenn kein Mapping gefunden
    public TextMeshProUGUI speakerNameText; // Optional: Anzeige des Sprecher-Namens

    [Header("Speaker Portraits")]
    [Tooltip("Zuordnung von Sprecher-Name zu Sprite.\nSprecher-Name entspricht dem ersten Wort in der Dialogzeile (vor ':').")]
    public List<SpeakerPortrait> speakerPortraits = new List<SpeakerPortrait>();

    // interne Felder
    private string[] lines;
    private int currentLine;
    private bool inDialogue;
    private bool aborted;

    public bool IsInDialogue => inDialogue;

    // Events
    public event Action OnDialogueComplete;
    public event Action OnDialogueAborted;

    [Serializable]
    public class SpeakerPortrait
    {
        public string speakerName; // z.B. "Maddison" (Case-sensitive oder -insensitive je nach Wunsch)
        public Sprite portrait;    // zugehöriges Sprite
    }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Stelle sicher, UI-Elemente deaktivieren
        dialogPanel.SetActive(false);
        nextButton.onClick.AddListener(OnNextButton);
        exitButton.onClick.AddListener(OnExitButton);
    }

    /// <summary>
    /// Startet einen Dialog mit übergebenem String-Array.
    /// </summary>
    public void StartDialogue(string[] dialogueLines)
    {
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
        if (currentLine < 0 || currentLine >= lines.Length) return;

        string rawLine = lines[currentLine];
        string speaker = null;
        string content = rawLine;

        // Versuch, Speaker und Inhalt zu trennen:
        // Falls Format "Name: Text..."
        int colonIndex = rawLine.IndexOf(':');
        if (colonIndex > 0)
        {
            speaker = rawLine.Substring(0, colonIndex).Trim();
            content = rawLine.Substring(colonIndex + 1).Trim();
        }
        else
        {
            // Kein ':' gefunden → nimm erstes Wort als Speaker, Rest als Content
            int spaceIndex = rawLine.IndexOf(' ');
            if (spaceIndex > 0)
            {
                speaker = rawLine.Substring(0, spaceIndex).Trim();
                content = rawLine.Substring(spaceIndex + 1).Trim();
            }
            else
            {
                // Ganze Zeile ist nur ein Wort → als Content anzeigen, Speaker unbekannt
                content = rawLine;
                speaker = null;
            }
        }

        // Setze Dialog-Text
        dialogText.text = content;

        // Setze Portrait
        if (portraitImage != null)
        {
            Sprite spriteToShow = defaultPortrait;
            if (!string.IsNullOrEmpty(speaker))
            {
                // Suche in der Liste nach Matching speakerName (case-insensitive)
                foreach (var sp in speakerPortraits)
                {
                    if (string.Equals(sp.speakerName, speaker, StringComparison.OrdinalIgnoreCase))
                    {
                        if (sp.portrait != null)
                            spriteToShow = sp.portrait;
                        break;
                    }
                }
            }
            // Setze Sprite und Aktivierung
            portraitImage.sprite = spriteToShow;
            portraitImage.gameObject.SetActive(spriteToShow != null);
        }

        // Setze Speaker-Name-Text, falls vorhanden
        if (speakerNameText != null)
        {
            if (!string.IsNullOrEmpty(speaker))
            {
                speakerNameText.gameObject.SetActive(true);
                speakerNameText.text = speaker;
            }
            else
            {
                speakerNameText.gameObject.SetActive(false);
            }
        }
    }

    private void OnNextButton()
    {
        if (!inDialogue) return;

        currentLine++;
        if (currentLine < lines.Length)
        {
            ShowLine();
        }
        else
        {
            // Ende
            EndDialogue();
            if (!aborted)
                OnDialogueComplete?.Invoke();
        }
    }

    private void OnExitButton()
    {
        if (!inDialogue) return;

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

        lines = null;
        currentLine = 0;
    }
}
