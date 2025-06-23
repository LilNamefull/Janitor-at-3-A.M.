using UnityEngine;

public class ExploreSceneManager : MonoBehaviour
{
    [Header("Initial Task")]
    [Tooltip("ID der Initial-Task")]
    public string initialTaskID = "SpeakGhost";
    [Tooltip("Titeltext der Initial-Task")]
    public string initialTaskTitle = "Task: Speak with the ghost";
    [Tooltip("Untertitel der Initial-Task (z.B. leer oder kurzer Hinweis)")]
    public string initialTaskSubtitle = "";

    void Start()
    {
        if (TaskUIManager.Instance != null)
        {
            TaskUIManager.Instance.ClearAllTasks();
            TaskUIManager.Instance.UpdateTask(initialTaskID, initialTaskTitle, initialTaskSubtitle);
        }
        else
        {
            Debug.LogWarning("[ExploreSceneManager] TaskUIManager.Instance ist null.");
        }
    }
}
