using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComicController : MonoBehaviour
{
    public Image comicImage;
    public Image[] whiteCovers;
    public Button comicNextButton;

    private int nextCoverIndex = 0;

    void Awake()
    {
        Debug.Log("[ComicController] Awake: " + gameObject.name + " activeSelf=" + gameObject.activeSelf);
        gameObject.SetActive(true);

        if (comicNextButton != null)
            comicNextButton.onClick.RemoveAllListeners();

        comicNextButton.onClick.AddListener(OnNextClick);

        for (int i = 0; i < whiteCovers.Length; i++)
        {
            if (whiteCovers[i] != null)
                whiteCovers[i].gameObject.SetActive(true);
            else
                Debug.LogWarning($"[ComicController] whiteCovers[{i}] ist null!");
        }

        nextCoverIndex = 0;
    }

    public void StartComic()
    {
        Debug.Log("[ComicController] StartComic() aufgerufen auf " + gameObject.name
                  + ", activeSelf vorher=" + gameObject.activeSelf);

        nextCoverIndex = 0;
        for (int i = 0; i < whiteCovers.Length; i++)
        {
            if (whiteCovers[i] != null)
                whiteCovers[i].gameObject.SetActive(true);
        }

        gameObject.SetActive(true);
        Debug.Log("[ComicController] StartComic(): activeSelf nach SetActive(true)=" + gameObject.activeSelf);
    }

    private void OnNextClick()
    {
        Debug.Log($"[ComicController] OnNextClick() – nextCoverIndex={nextCoverIndex}");
        if (nextCoverIndex < whiteCovers.Length && whiteCovers[nextCoverIndex] != null)
        {
            whiteCovers[nextCoverIndex].gameObject.SetActive(false);
            nextCoverIndex++;
        }
        else
        {
            Debug.LogWarning($"[ComicController] Kein Cover an Index {nextCoverIndex}");
        }

        if (nextCoverIndex >= whiteCovers.Length)
        {
            Debug.Log("[ComicController] Alle Cover weg, Panel wird deaktiviert");
            gameObject.SetActive(false);
        }
    }
}

