using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance;

    // Interne Repräsentation einer Task
    private class TaskData
    {
        public string id;
        public string title;
        public int currentCount;
        public int totalCount; // =0 für non-quantitative
        public string subtitle;
    }

    private Dictionary<string, TaskData> tasks = new Dictionary<string, TaskData>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            Debug.Log("[TaskManager] Awake: Instance gesetzt.");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// Fügt oder aktualisiert eine non-quantitative Task (ohne Zähler).
    /// </summary>
    public void AddTask(string taskID, string title, string subtitle = "")
    {
        if (string.IsNullOrEmpty(taskID)) return;
        if (tasks.ContainsKey(taskID))
        {
            var td = tasks[taskID];
            td.title = title;
            td.subtitle = subtitle;
        }
        else
        {
            var td = new TaskData
            {
                id = taskID,
                title = title,
                currentCount = 0,
                totalCount = 0,
                subtitle = subtitle
            };
            tasks[taskID] = td;
        }
        TaskUIManager.Instance?.UpdateTask(taskID, title, tasks[taskID].subtitle);
        Debug.Log($"[TaskManager] AddTask non-quantitative id={taskID}, title={title}, subtitle={subtitle}");
    }

    /// <summary>
    /// Fügt oder aktualisiert eine quantitative Task mit totalCount > 0, initial currentCount=0.
    /// </summary>
    public void AddTask(string taskID, string title, int totalCount)
    {
        if (string.IsNullOrEmpty(taskID) || totalCount <= 0) return;
        if (tasks.ContainsKey(taskID))
        {
            var td = tasks[taskID];
            td.title = title;
            td.totalCount = totalCount;
            td.currentCount = 0;
        }
        else
        {
            var td = new TaskData
            {
                id = taskID,
                title = title,
                currentCount = 0,
                totalCount = totalCount,
                subtitle = "" // wird unten gesetzt
            };
            tasks[taskID] = td;
        }
        string sub = $"0/{totalCount}";
        tasks[taskID].subtitle = sub;
        TaskUIManager.Instance?.UpdateTask(taskID, title, sub);
        Debug.Log($"[TaskManager] AddTask quantitative id={taskID}, title={title}, subtitle={sub}");
    }

    /// <summary>
    /// Erhöht den Fortschritt einer quantitativen Task; entfernt die Task, falls erledigt (currentCount >= totalCount).
    /// </summary>
    public bool IncrementTask(string taskID, int amount = 1)
    {
        if (!tasks.ContainsKey(taskID)) return false;
        var td = tasks[taskID];
        if (td.totalCount <= 0) return false;
        td.currentCount += amount;
        if (td.currentCount >= td.totalCount)
        {
            RemoveTask(taskID);
            Debug.Log($"[TaskManager] Task {taskID} erledigt und entfernt.");
            return true;
        }
        else
        {
            td.subtitle = $"{td.currentCount}/{td.totalCount}";
            TaskUIManager.Instance?.UpdateTask(taskID, td.title, td.subtitle);
            Debug.Log($"[TaskManager] Task {taskID} Fortschritt: {td.subtitle}");
            return false;
        }
    }

    /// <summary>
    /// Entfernt eine Task, falls vorhanden.
    /// </summary>
    public void RemoveTask(string taskID)
    {
        if (string.IsNullOrEmpty(taskID)) return;
        if (tasks.Remove(taskID))
        {
            TaskUIManager.Instance?.RemoveTask(taskID);
            Debug.Log($"[TaskManager] RemoveTask id={taskID}");
        }
    }

    /// <summary>
    /// Löscht alle Tasks.
    /// </summary>
    public void ClearAllTasks()
    {
        tasks.Clear();
        TaskUIManager.Instance?.ClearAllTasks();
        Debug.Log("[TaskManager] ClearAllTasks aufgerufen.");
    }

    /// <summary>
    /// Prüft, ob eine Task aktiv ist.
    /// </summary>
    public bool HasTask(string taskID)
    {
        return !string.IsNullOrEmpty(taskID) && tasks.ContainsKey(taskID);
    }

    /// <summary>
    /// Bei Szenenwechsel: in Chase-Szene „RUN“ setzen.
    /// Passe den Szenen-Namen an deine Chase-Szene an.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "ChaseScene") // Passe den exakten Namen deiner Chase-Szene an
        {
            ClearAllTasks();
            AddTask("Run", "Task: RUN", "");
            Debug.Log("[TaskManager] ChaseScene geladen: Task RUN hinzugefügt.");
        }
        // Weitere Szenen-Initialisierung hier falls nötig.
    }
}
