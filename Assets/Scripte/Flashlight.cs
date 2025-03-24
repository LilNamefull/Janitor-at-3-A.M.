using UnityEngine;

public class Flashlight : MonoBehaviour
{
    private Light flashlight;
    private bool isOn = false;

    void Start()
    {
        flashlight = GetComponentInChildren<Light>();
        flashlight.enabled = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F)) // Mit "F" ein-/ausschalten
        {
            isOn = !isOn;
            flashlight.enabled = isOn;
        }
    }
}
