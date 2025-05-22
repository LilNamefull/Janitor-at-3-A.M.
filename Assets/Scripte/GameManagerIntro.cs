using UnityEngine;

public class GameManagerIntro : MonoBehaviour
{
    public static GameManagerIntro Instance;

    [Header("Level-Aufgaben")]
    public int totalSpots;       // Anzahl der Flecken im Level
    public int totalChairs;      // Anzahl der aufzuräumenden Stühle
    [HideInInspector] public int cleanedSpots = 0;
    [HideInInspector] public int placedChairs = 0;

    [Header("Finales Event")]
    public LockerCutsceneController lockerController; // Inspector: dein LockerCutsceneController

    [HideInInspector]
    public bool allTasksDone = false;  // true, sobald beide Minispiele abgeschlossen sind

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
        Debug.Log($"CheckAllDone: cleanedSpots={cleanedSpots}/{totalSpots}, placedChairs={placedChairs}/{totalChairs}");
        if (cleanedSpots >= totalSpots && placedChairs >= totalChairs)
        {
            Debug.Log("Alle Aufgaben erledigt → Klopfen wird gespielt!");
            allTasksDone = true;
            
           
        }
      
    }
}
