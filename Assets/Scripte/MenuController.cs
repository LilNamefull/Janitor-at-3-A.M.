using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public GameObject controlsPanel;

    void Start()
    {
       
        if (controlsPanel != null) controlsPanel.SetActive(false);
    }
    // Wird vom „Play“-Button aufgerufen
    public void OnPlayPressed()
    {
        SceneManager.LoadScene("ComicIntro");
    }

    public void OnControlsButton()
    {
        
        if (controlsPanel != null) controlsPanel.SetActive(true);
    }

    public void OnBackFromControls()
    {
        // Verstecke Controls-Panel, zeige Hauptmenü-Panel
        if (controlsPanel != null) controlsPanel.SetActive(false);
       
    }

    // Wird vom „Ende“-Button aufgerufen
    public void OnQuitPressed()
    {
        Application.Quit();
    }
}
