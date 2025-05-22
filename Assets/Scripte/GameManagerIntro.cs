using UnityEngine;

public class GameManagerIntro : MonoBehaviour
{
    public static GameManagerIntro Instance;

    [Header("Aufgaben")]
    public int totalSpots;
    public int totalChairs;
    [HideInInspector] public int cleanedSpots = 1;
    [HideInInspector] public int placedChairs = 1;

    [Header("Locker Cutscene")]
    public LockerCutsceneController lockerController;

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
        if (!allTasksDone && cleanedSpots >= totalSpots && placedChairs >= totalChairs)
        {
            allTasksDone = true;
            Debug.Log("Alle Aufgaben erledigt → Klopfen starten");
            if (lockerController != null)
                lockerController.PlayKnock();
            else
                Debug.LogError("lockerController nicht zugewiesen!");
        }
    }
}
