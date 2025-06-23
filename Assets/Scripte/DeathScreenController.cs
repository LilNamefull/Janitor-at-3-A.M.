using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DeathScreenController : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Panel, das bei Tod aktiviert wird.")]
    public GameObject deathPanel;
    [Tooltip("Button ‚Retry‘: lädt die aktuelle Szene neu.")]
    public Button retryButton;
    [Tooltip("Button ‚Main Menu‘: lädt die Hauptmenü-Szene.")]
    public Button menuButton;

    public GameObject hotbarUI;

    [Header("Settings")]
    [Tooltip("Name der Szene für das Hauptmenü.")]
    public string mainMenuSceneName = "MainMenu";
    // Optional: Wenn du eine bestimmte Chase-Szene hast, die immer neu gestartet wird, kannst du hier den Namen eintragen.
    // Wenn leer, wird generell die aktuell geladene Szene neu gestartet.
    public string defaultRetrySceneName = ""; // Wenn leer, wird aktuelle Szene neu geladen.

    // Singleton-Instanz (optional, falls du einfach von überall darauf zugreifen willst)
    public static DeathScreenController Instance { get; private set; }

    void Awake()
    {

        
        // Singleton-Pattern: Wenn du nur eine Instanz brauchst
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: über Szenenwechsel erhalten? Wenn DeathScreen nur pro Szene ist, entferne DontDestroyOnLoad.
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        if (hotbarUI != null) hotbarUI.SetActive(true);
        


        // Panel und Buttons initial konfigurieren
        if (deathPanel != null)
            deathPanel.SetActive(false);

        if (retryButton != null)
        {
            retryButton.onClick.RemoveAllListeners();
            retryButton.onClick.AddListener(OnRetryPressed);
        }
        if (menuButton != null)
        {
            menuButton.onClick.RemoveAllListeners();
            menuButton.onClick.AddListener(OnMenuPressed);
        }
    }

    /// <summary>
    /// Wird aufgerufen, wenn der Spieler stirbt.
    /// </summary>
    public void ShowDeathScreen()
    {
        if (hotbarUI != null)
            hotbarUI.SetActive(false);
        // Zeit anhalten, Cursor freigeben etc.
        Time.timeScale = 0f;
        // Maus freigeben
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (deathPanel != null)
            deathPanel.SetActive(true);
    }

    private void OnRetryPressed()
    {
        // DeathPanel ausblenden und Zeit zurücksetzen
        if (deathPanel != null)
            deathPanel.SetActive(false);
        Time.timeScale = 1f;

        // Cursor wieder sperren (optional, je nach deinem Input-System)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Szene neu laden:
        string sceneToLoad = defaultRetrySceneName;
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            // Wenn kein chaseSceneName angegeben, lade die aktuell aktive Szene neu:
            sceneToLoad = SceneManager.GetActiveScene().name;
        }
        SceneManager.LoadScene(sceneToLoad);
        GameManager.Instance.ResetKeys();
    }

    private void OnMenuPressed()
    {
        // DeathPanel ausblenden und Zeit zurücksetzen (optional)
        if (deathPanel != null)
            deathPanel.SetActive(false);
        Time.timeScale = 1f;

        // Cursor sichtbar und freigegeben, damit im Main Menu UI angeklickt werden kann
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Lade Main Menu Szene
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            GameManager.Instance.ResetKeys();
            SceneManager.LoadScene(mainMenuSceneName);
            // Hier kannst du bei Bedarf noch Reset-Logik hinzufügen, falls du persistente Daten löschen willst.
        }
        else
        {
            Debug.LogWarning("[DeathScreenController] mainMenuSceneName ist leer!");
        }
    }
}
