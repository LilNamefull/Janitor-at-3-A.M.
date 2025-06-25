using UnityEngine;
using UnityEngine.SceneManagement;

public class Level1Manager : MonoBehaviour
{
    public static Level1Manager Instance;
    private const string CleanSpotsID = "CleanSpots";
    private const string PlaceChairsID = "PlaceChairs";

    [Header("Anzahl Spots/Chairs")]
    public int totalSpots = 4;
    public int totalChairs = 5;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Stelle sicher, dass TaskManager existiert; wenn nicht, erstelle ihn automatisch.
        if (TaskManager.Instance == null)
        {
            Debug.LogWarning("[Level1Manager] TaskManager.Instance ist null. Erstelle TaskManager automatisch.");
            GameObject go = new GameObject("TaskManager");
            go.AddComponent<TaskManager>();
            // TaskManager.Awake() setzt dann Instance.
        }
        else
        {
            Debug.Log("[Level1Manager] TaskManager.Instance bereits vorhanden.");
        }

        // Falls die aktuell geladene Szene direkt „HeistJanitorLvl“ ist, initialisiere Tasks sofort:
        if (SceneManager.GetActiveScene().name == "JanitorLvl")
        {
            InitializeTasks();
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Wenn die HeistJanitorLvl-Szene geladen wird, initialisiere Tasks
        if (scene.name == "JanitorLvl")
        {
            // Stelle sicher, TaskManager existiert
            if (TaskManager.Instance == null)
            {
                Debug.LogWarning("[Level1Manager] OnSceneLoaded: TaskManager.Instance noch null. Erstelle erneut.");
                GameObject go = new GameObject("TaskManager");
                go.AddComponent<TaskManager>();
            }
            InitializeTasks();
        }
    }

    private void InitializeTasks()
    {
        Debug.Log("[Level1Manager] InitializeTasks aufgerufen");
        if (TaskManager.Instance == null)
        {
            Debug.LogError("[Level1Manager] TaskManager.Instance ist null, breche InitializeTasks ab!");
            return;
        }
        if (TaskUIManager.Instance == null)
        {
            Debug.LogWarning("[Level1Manager] TaskUIManager.Instance ist null!");
        }

        TaskManager.Instance.ClearAllTasks();
        TaskManager.Instance.AddTask(CleanSpotsID, "Task: Clean the spots", totalSpots);
        TaskManager.Instance.AddTask(PlaceChairsID, "Task: Place the chairs", totalChairs);
    }

    public void OnSpotCleaned()
    {
        Debug.Log("[Level1Manager] OnSpotCleaned aufgerufen");
        if (TaskManager.Instance != null)
        {
            bool done = TaskManager.Instance.IncrementTask(CleanSpotsID, 1);
            Debug.Log($"[Level1Manager] CleanSpots incrementiert, done={done}");
        }
        CheckInitialDone();
    }

    public void OnChairPlaced()
    {
        Debug.Log("[Level1Manager] OnChairPlaced aufgerufen");
        if (TaskManager.Instance != null)
        {
            bool done = TaskManager.Instance.IncrementTask(PlaceChairsID, 1);
            Debug.Log($"[Level1Manager] PlaceChairs incrementiert, done={done}");
        }
        CheckInitialDone();
    }

    private void CheckInitialDone()
    {
        if (TaskManager.Instance == null) return;
        bool stillHasClean = TaskManager.Instance.HasTask(CleanSpotsID);
        bool stillHasChair = TaskManager.Instance.HasTask(PlaceChairsID);
        Debug.Log($"[Level1Manager] CheckInitialDone: Clean noch da? {stillHasClean}, Chair noch da? {stillHasChair}");
        if (!stillHasClean && !stillHasChair)
        {
            const string investigateID = "InvestigateNoise";
            if (!TaskManager.Instance.HasTask(investigateID))
            {
                Debug.Log("[Level1Manager] Beides erledigt – setze InvestigateNoise");
                TaskManager.Instance.AddTask(
                    investigateID,
                    "Task: Investigate the noise",
                    "It sounds like it's coming from the main entrance"
                );
            }
        }
    }
}
