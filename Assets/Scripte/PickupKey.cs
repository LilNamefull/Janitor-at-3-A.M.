using UnityEngine;

public class PickupKey : MonoBehaviour
{
    private bool collected = false;

    [Header("Task-IDs, die entfernt werden sollen, wenn dieser Key eingesammelt wird")]
    [Tooltip("IDs der Tasks, die vor dem Key-Einsammeln vom NPC gesetzt wurden und nun weg sollen")]
    public string[] taskIDsToRemoveOnPickup;

    private void OnTriggerEnter(Collider other)
    {
        if (collected) return;

        if (other.CompareTag("Player"))
        {
            collected = true;

            // 1) Zähle den Key im GameManager
            GameManager.Instance.CollectKey();

            // 2) Entferne NPC-Tasks, die vor dem Einsammeln da waren
            RemoveNPCTasks();

            // 3) Aktualisiere/demonstriere Key-Task-Logik
            UpdateKeyTask();

            // 4) Key-Objekt zerstören
            Destroy(gameObject);
        }
    }

    private void RemoveNPCTasks()
    {
        if (TaskUIManager.Instance == null) return;
        if (taskIDsToRemoveOnPickup == null) return;

        foreach (var taskID in taskIDsToRemoveOnPickup)
        {
            if (!string.IsNullOrEmpty(taskID))
            {
                TaskUIManager.Instance.RemoveTask(taskID);
            }
        }
    }

    private void UpdateKeyTask()
    {
        if (TaskUIManager.Instance == null) return;

        int keys = GameManager.Instance.keysCollected;
        int total = GameManager.Instance.totalKeysRequired;

        if (keys < total)
        {
            // Beispiel: Zeige Task „Collect keys X/Y“
            string id = "CollectKeys";
            string title = $"Task: Collect keys {keys}/{total}";
            string subtitle = "Find the next key";
            TaskUIManager.Instance.UpdateTask(id, title, subtitle);
        }
        else
        {
            // Alle Keys gesammelt: entferne CollectKeys-Task, zeige UnlockDoor-Task
            TaskUIManager.Instance.RemoveTask("CollectKeys");
            string id2 = "UnlockDoor";
            string title2 = "Task: Unlock the door";
            string subtitle2 = "Use the keys at the locked door";
            TaskUIManager.Instance.UpdateTask(id2, title2, subtitle2);
        }
    }
}
