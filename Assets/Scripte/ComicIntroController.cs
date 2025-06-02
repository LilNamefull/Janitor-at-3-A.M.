using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ComicIntroController : MonoBehaviour
{
    [Tooltip("Referenz auf das GameObject, an dem ComicController h‰ngt.")]
    public ComicController comicController;

    [Tooltip("Name der Szene, die nach dem Comic startet.")]
    public string miniGameSceneName = "JanitorLvl";

    void Start()
    {
         if (comicController != null)
            {
                // ComicController aktiviert das Panel mit weiﬂen Cover-K‰sten
                comicController.StartComic();
                StartCoroutine(WaitForComicEnd());
            }
            else
            {
                Debug.LogError("[ComicIntro] comicController fehlt!");
            }
    }

    private IEnumerator WaitForComicEnd()
    {
        // Solange das ComicPanel aktiv ist, warte
        while (comicController.gameObject.activeSelf)
            yield return null;

        // Sobald ComicPanel deaktiviert ist (alle Covers weggeklickt), lade MiniGame
        SceneManager.LoadScene(miniGameSceneName);
    }
}
