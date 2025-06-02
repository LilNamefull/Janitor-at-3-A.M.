using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    // Wird vom „Play“-Button aufgerufen
    public void OnPlayPressed()
    {
        SceneManager.LoadScene("ComicIntro");
    }

    // Wird vom „Ende“-Button aufgerufen
    public void OnQuitPressed()
    {
        Application.Quit();
    }
}
