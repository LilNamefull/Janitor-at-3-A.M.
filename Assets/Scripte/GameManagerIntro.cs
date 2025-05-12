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
    public LockerController lockerController; // Inspector: dein Locker-Objekt

    void Awake()
    {
        // Singleton-Setup
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>Wird aufgerufen, wenn ein Fleck vollständig gereinigt wurde.</summary>
    public void SpotCleaned()
    {
        cleanedSpots++;
        CheckAllDone();
    }

    /// <summary>Wird aufgerufen, wenn ein Stuhl korrekt platziert wurde.</summary>
    public void ChairPlaced()
    {
        placedChairs++;
        CheckAllDone();
    }

    /// <summary>Prüft, ob alle Aufgaben erledigt sind und startet das Finale.</summary>

        private void CheckAllDone()
        {
            Debug.Log($"CheckAllDone: cleanedSpots={cleanedSpots}/{totalSpots}, placedChairs={placedChairs}/{totalChairs}");
            if (cleanedSpots >= totalSpots && placedChairs >= totalChairs)
            {
                Debug.Log(">>> ALL DONE! LockerController wird gestartet.");
                if (lockerController != null)
                {
                    lockerController.StartLockering();
                }
                else
                {
                    Debug.LogError("GameManagerIntro: lockerController ist NULL!");
                }
            }
        }

    
}
