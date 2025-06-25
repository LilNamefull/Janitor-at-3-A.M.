using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PauseMenuController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pausePanel;       // Das Panel, das alle Buttons und Slider enthält
    public Button resumeButton;
    public Button restartButton;
    public Button mainMenuButton;
    public Slider sensitivitySlider;
    public TextMeshProUGUI sensitivityValueText;   // Text, der den aktuellen Wert anzeigt

    [Header("Settings")]
    public string mainMenuSceneName = "MainMenu"; // Name der Szene für Hauptmenü
    public float defaultSensitivity = 1.0f;       // Fallback, wenn kein gespeicherter Wert existiert
    public float minSensitivity = 0.1f;
    public float maxSensitivity = 5.0f;

    // Aktueller Wert, statisch, damit andere Skripte leicht darauf zugreifen können:
    public static float MouseSensitivity = 1.0f;

    private bool isPaused = false;

    void Awake()
    {
        // Singleton-Pattern: sicherstellen, dass nur eine Instanz persistent ist
        if (FindObjectsOfType<PauseMenuController>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        // Lade gespeicherte Empfindlichkeit
        MouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", defaultSensitivity);

        // UI initialisieren, aber warte, bis UI referenzen gesetzt sind
        // pausePanel und andere Referenzen sollten im Inspector zugewiesen werden.
    }

    void Start()
    {
        if (pausePanel == null) Debug.LogError("PauseMenuController: pausePanel fehlt!");
        // Setze Panel inaktiv
        pausePanel.SetActive(false);

        // Buttons
        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveAllListeners();
            resumeButton.onClick.AddListener(ResumeGame);
        }
        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(RestartScene);
        }
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        }
        // Slider
        if (sensitivitySlider != null)
        {
            sensitivitySlider.minValue = minSensitivity;
            sensitivitySlider.maxValue = maxSensitivity;
            sensitivitySlider.value = MouseSensitivity;
            sensitivitySlider.onValueChanged.RemoveAllListeners();
            sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
        }
        UpdateSensitivityText();
    }

    void Update()
    {
        // ESC toggelt Pause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    private void PauseGame()
    {
        isPaused = true;
        // Zeige UI
        if (pausePanel != null)
            pausePanel.SetActive(true);
        // Zeit anhalten
        Time.timeScale = 0f;
        // Cursor sichtbar
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ResumeGame()
    {
        isPaused = false;
        if (pausePanel != null)
            pausePanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void RestartScene()
    {
        // Resume und lade aktuelle Szene neu
        isPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        // Cursor ggf. sperren
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        GameManager.Instance.ResetKeys();
    }

    private void GoToMainMenu()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            GameManager.Instance.ResetKeys();
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
            Debug.LogWarning("PauseMenuController: mainMenuSceneName nicht gesetzt!");
    }

    private void OnSensitivityChanged(float val)
    {
        MouseSensitivity = val;
        PlayerPrefs.SetFloat("MouseSensitivity", MouseSensitivity);
        PlayerPrefs.Save();
        UpdateSensitivityText();
        Debug.Log($"[PauseMenu] MouseSensitivity gesetzt auf {MouseSensitivity:F2}");
    }

    private void UpdateSensitivityText()
    {
        if (sensitivityValueText != null)
            sensitivityValueText.text = MouseSensitivity.ToString("F2");
    }
}
