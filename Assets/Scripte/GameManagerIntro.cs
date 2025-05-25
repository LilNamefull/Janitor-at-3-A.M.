using UnityEngine;

public class GameManagerIntro : MonoBehaviour
{
    public static GameManagerIntro Instance;

    [Header("Level-Aufgaben")]
    public int totalSpots;
    public int totalChairs;
    [HideInInspector] public int cleanedSpots = 1;
    [HideInInspector] public int placedChairs = 1;

    [HideInInspector] public bool allTasksDone = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SpotCleaned()
    {
        cleanedSpots++;
        CheckAllDone();
    }

    public void ChairPlaced()
    {
        placedChairs++;
        CheckAllDone();
    }

    private void CheckAllDone()
    {
        Debug.Log($"[GameManager] Spots: {cleanedSpots}/{totalSpots}, Chairs: {placedChairs}/{totalChairs}");
        if (!allTasksDone && cleanedSpots >= totalSpots && placedChairs >= totalChairs)
        {
            allTasksDone = true;
            Debug.Log("[GameManager] Alle Aufgaben erledigt!");
            // kein PlayKnock() mehr – KnockAndDialogController startet selbst
        }
    }
}
