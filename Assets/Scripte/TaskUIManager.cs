using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TaskUIManager : MonoBehaviour
{
    public static TaskUIManager Instance;

    [Header("UI Settings")]
    [Tooltip("Content-Parent im Canvas, unter dem die Task-Einträge erscheinen")]
    public RectTransform taskListParent;  // z.B. TaskPanel im Canvas

    [Tooltip("Prefab für einen Task-Eintrag (muss TaskEntryUI-Komponente enthalten)")]
    public GameObject taskEntryPrefab;

    // Intern: Map von taskID zu Instanz-GameObject
    private Dictionary<string, GameObject> entries = new Dictionary<string, GameObject>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // üblicherweise bleibt TaskUIManager in jeder Szene, hier evtl. nicht persistent
            Debug.Log("[TaskUIManager] Awake: bereit");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Aktualisiert oder fügt eine Task-Eintrag hinzu.
    /// Wird aufgerufen von TaskManager.
    /// </summary>
    public void UpdateTask(string taskID, string title, string subtitle)
    {
        if (string.IsNullOrEmpty(taskID) || taskListParent == null || taskEntryPrefab == null)
        {
            Debug.LogWarning("[TaskUIManager] UpdateTask: fehlende Referenzen oder ungültige taskID.");
            return;
        }

        if (entries.ContainsKey(taskID))
        {
            // Bereits existierender Eintrag: Text aktualisieren
            GameObject entryGO = entries[taskID];
            TaskEntryUI entryUI = entryGO.GetComponent<TaskEntryUI>();
            if (entryUI != null)
            {
                entryUI.SetText(title, subtitle);
                Debug.Log($"[TaskUIManager] UpdateTask id={taskID}, title={title}, subtitle={subtitle}");
            }
            else
            {
                Debug.LogWarning($"[TaskUIManager] UpdateTask: TaskEntryUI-Komponente fehlt an Objekt {entryGO.name}");
            }
        }
        else
        {
            // Neuer Eintrag: Instanziere Prefab unter parent
            GameObject entryGO = Instantiate(taskEntryPrefab, taskListParent);
            entryGO.name = "Task_" + taskID;
            TaskEntryUI entryUI = entryGO.GetComponent<TaskEntryUI>();
            if (entryUI != null)
            {
                entryUI.SetText(title, subtitle);
            }
            else
            {
                Debug.LogWarning($"[TaskUIManager] UpdateTask: TaskEntryUI-Komponente fehlt im Prefab {taskEntryPrefab.name}");
            }
            entries[taskID] = entryGO;
            Debug.Log($"[TaskUIManager] Neuer Task-Eintrag erstellt: id={taskID}");
        }
    }

    /// <summary>
    /// Entfernt eine Task-Eintrag aus dem UI.
    /// </summary>
    public void RemoveTask(string taskID)
    {
        if (string.IsNullOrEmpty(taskID)) return;
        if (entries.ContainsKey(taskID))
        {
            GameObject entryGO = entries[taskID];
            entries.Remove(taskID);
            Destroy(entryGO);
            Debug.Log($"[TaskUIManager] RemoveTask id={taskID}");
        }
    }

    /// <summary>
    /// Entfernt alle Task-Einträge.
    /// </summary>
    public void ClearAllTasks()
    {
        foreach (var kv in entries)
        {
            if (kv.Value != null)
                Destroy(kv.Value);
        }
        entries.Clear();
        Debug.Log("[TaskUIManager] ClearAllTasks");
    }
}
