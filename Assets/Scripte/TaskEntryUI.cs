using TMPro;
using UnityEngine;
using TMPro;

public class TaskEntryUI : MonoBehaviour
{
    [Header("Referenzen zu den UI-Texten")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI subtitleText;

    /// <summary>
    /// Setzt Titel und Untertitel im Eintrag.
    /// </summary>
    public void SetText(string title, string subtitle)
    {
        if (titleText != null)
            titleText.text = title;
        if (subtitleText != null)
            subtitleText.text = subtitle;
    }
}
