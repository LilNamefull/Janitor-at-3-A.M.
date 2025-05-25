using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManagerIntro : MonoBehaviour
{
    public static GameManagerIntro Instance;

    [Header("Level-Aufgaben")]
    public int totalSpots;
    public int totalChairs;
    [HideInInspector] public int cleanedSpots = 1;
    [HideInInspector] public int placedChairs = 1;

    [HideInInspector] public bool allTasksDone = false;

    [Header("UI (oben links)")]
    public TextMeshProUGUI spotsText;      // Text-Element für Flecken
    public TextMeshProUGUI chairsText;     // Text-Element für Stühle

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Direkt initialisieren
        UpdateUIText();
    }

    public void SpotCleaned()
    {
        cleanedSpots++;
        CheckAllDone();
        UpdateUIText();
    }

    public void ChairPlaced()
    {
        placedChairs++;
        CheckAllDone();
        UpdateUIText();
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
    private void UpdateUIText()
    {
        if (spotsText != null)
            spotsText.text = $"Spots: {cleanedSpots}/{totalSpots}";
        if (chairsText != null)
            chairsText.text = $"Chairs: {placedChairs}/{totalChairs}";
    }
}
