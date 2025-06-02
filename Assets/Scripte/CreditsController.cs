using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsController : MonoBehaviour
{
    [Tooltip("Name der Hauptmenü-Szene")]
    public string mainMenuSceneName = "MainMenu";

    // Wird vom „Menu“-Button aufgerufen
    public void OnMenuPressed()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
