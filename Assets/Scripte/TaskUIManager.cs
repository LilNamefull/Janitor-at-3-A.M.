using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class TaskUIManager : MonoBehaviour
{
    public static TaskUIManager Instance;

    [Header("UI References")]
    [Tooltip("Parent für Task-Einträge (z.B. TaskPanel Transform)")]
    public Transform taskListParent;

    [Tooltip("Prefab für einen Task-Eintrag. Erwartet: unter Root zwei TMP-Text-Objekte namens TitleText und SubtitleText oder per TaskEntryUI referenziert.")]
    public GameObject taskEntryPrefab;

    // Interne Map: Task-ID → Instanz des Eintrags
    private Dictionary<string, GameObject> activeTasks = new Dictionary<string, GameObject>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Optional: Falls TaskUIManager in allen Szenen persistieren soll:
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (taskListParent == null)
            Debug.LogError("[TaskUIManager] taskListParent nicht gesetzt!");
        if (taskEntryPrefab == null)
            Debug.LogError("[TaskUIManager] taskEntryPrefab nicht gesetzt!");

        // Optional: TaskPanel (parent) initial ausblenden, falls kein Task aktiv:
        UpdatePanelVisibility();
    }

    /// <summary>
    /// Fügt eine neue Task hinzu (oder aktualisiert sie, falls ID bereits existiert).
    /// </summary>
    /// <param name="taskID">Eindeutige ID, z.B. "SpeakGhost"</param>
    /// <param name="title">Titeltext, z.B. "Task: Speak with the ghost"</param>
    /// <param name="subtitle">Untertiteltext, z.B. "He should be under the stairway"</param>
    public void UpdateTask(string taskID, string title, string subtitle)
    {
        if (string.IsNullOrEmpty(taskID))
        {
            Debug.LogWarning("[TaskUIManager] UpdateTask: taskID ist null oder leer.");
            return;
        }

        if (activeTasks.ContainsKey(taskID))
        {
            // Aktualisiere bestehenden Eintrag
            GameObject entry = activeTasks[taskID];
            ApplyTextsToEntry(entry, title, subtitle);
        }
        else
        {
            // Neuer Eintrag
            AddTask(taskID, title, subtitle);
        }
        UpdatePanelVisibility();
    }

    /// <summary>
    /// Fügt eine Task hinzu, wenn sie noch nicht existiert.
    /// </summary>
    public void AddTask(string taskID, string title, string subtitle)
    {
        if (string.IsNullOrEmpty(taskID))
        {
            Debug.LogWarning("[TaskUIManager] AddTask: taskID ist null oder leer.");
            return;
        }
        if (activeTasks.ContainsKey(taskID))
        {
            Debug.LogWarning($"[TaskUIManager] Task '{taskID}' existiert bereits. Nutze UpdateTask, wenn du Text ändern möchtest.");
            return;
        }
        if (taskEntryPrefab == null || taskListParent == null)
        {
            Debug.LogError("[TaskUIManager] Prefab oder Parent fehlt.");
            return;
        }

        GameObject entry = Instantiate(taskEntryPrefab, taskListParent);
        ApplyTextsToEntry(entry, title, subtitle);
        activeTasks[taskID] = entry;
        UpdatePanelVisibility();
    }

    /// <summary>
    /// Entfernt eine Task, falls vorhanden.
    /// </summary>
    public void RemoveTask(string taskID)
    {
        if (string.IsNullOrEmpty(taskID)) return;
        if (activeTasks.TryGetValue(taskID, out GameObject entry))
        {
            Destroy(entry);
            activeTasks.Remove(taskID);
            UpdatePanelVisibility();
        }
    }

    /// <summary>
    /// Entfernt alle Tasks.
    /// </summary>
    public void ClearAllTasks()
    {
        foreach (var kv in activeTasks)
        {
            if (kv.Value != null)
                Destroy(kv.Value);
        }
        activeTasks.Clear();
        UpdatePanelVisibility();
    }

    /// <summary>
    /// Prüft, ob eine Task mit dieser ID existiert.
    /// </summary>
    public bool InstanceHasTask(string taskID)
    {
        return !string.IsNullOrEmpty(taskID) && activeTasks.ContainsKey(taskID);
    }

    /// <summary>
    /// Setzt Titel- und Untertitel-Text in einem TaskEntry-Objekt.
    /// Erwartet unter entry Transform: Kind namens "TitleText" mit TMP-Component, und "SubtitleText".
    /// Falls anders, versucht GetComponentInChildren.
    /// </summary>
    private void ApplyTextsToEntry(GameObject entry, string title, string subtitle)
    {
        if (entry == null) return;
        TextMeshProUGUI titleText = null;
        TextMeshProUGUI subtitleText = null;

        // Versuche, über benannte Kinder zu finden:
        var tChild = entry.transform.Find("TitleText");
        if (tChild != null) titleText = tChild.GetComponent<TextMeshProUGUI>();
        var sChild = entry.transform.Find("SubtitleText");
        if (sChild != null) subtitleText = sChild.GetComponent<TextMeshProUGUI>();

        // Fallback: alle TMP-Komponenten im Kind
        if (titleText == null || subtitleText == null)
        {
            var texts = entry.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length >= 2)
            {
                // Annahme: erstes ist Title, zweites Subtitle
                titleText = texts[0];
                subtitleText = texts[1];
            }
        }

        if (titleText != null)
            titleText.text = title ?? "";
        if (subtitleText != null)
            subtitleText.text = subtitle ?? "";
    }

    /// <summary>
    /// Blendet das TaskPanel (Parent) aus, wenn keine Tasks aktiv sind, sonst ein.
    /// </summary>
    private void UpdatePanelVisibility()
    {
        if (taskListParent == null) return;
        GameObject panelGO = taskListParent.gameObject;
        // Falls Parent direkt TaskPanel ist. Falls TaskListParent das Content-Element ist,
        // nimm dessen Parent: panelGO = taskListParent.parent.gameObject;
        // Passe hier an, je nachdem, ob taskListParent direkt das Panel oder Content ist.
        // Wir gehen davon aus, taskListParent ist das Panel selbst.
        panelGO.SetActive(activeTasks.Count > 0);
    }
}

