using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class LampBlinker : MonoBehaviour
{
    [Header("Blink Settings")]
    [Tooltip("Minimale Wartezeit (in Sekunden) zwischen den Zustandsänderungen.")]
    public float minInterval = 0.1f;
    [Tooltip("Maximale Wartezeit (in Sekunden) zwischen den Zustandsänderungen.")]
    public float maxInterval = 1.0f;

    [Tooltip("Intensität, wenn die Lampe EIN ist.")]
    public float onIntensity = 3f;

    private Light lampLight;
    private bool isOn = false;

    void Awake()
    {
        lampLight = GetComponent<Light>();
        // Starte mit ausgeschalteter Lampe
        lampLight.intensity = 0f;
        isOn = false;
    }

    void OnEnable()
    {
        // Sobald das GameObject aktiv wird, starte die Blinker‐Routine
        StartCoroutine(BlinkCoroutine());
    }

    void OnDisable()
    {
        // Wenn das GameObject deaktiviert oder zerstört wird, stoppe die Coroutine
        StopAllCoroutines();
    }

    private IEnumerator BlinkCoroutine()
    {
        while (true)
        {
            // Zufällige Zeit abwarten, bevor wir den Zustand umschalten
            float waitTime = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(waitTime);

            // Schalte um: Wenn aktuell aus, dann an; sonst aus
            if (isOn)
            {
                lampLight.intensity = 0f;
                isOn = false;
            }
            else
            {
                lampLight.intensity = onIntensity;
                isOn = true;
            }
        }
    }
}
