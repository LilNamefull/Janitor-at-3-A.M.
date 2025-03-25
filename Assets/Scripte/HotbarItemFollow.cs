using UnityEngine;

public class HotbarItemFollow : MonoBehaviour
{
    public Transform cameraTransform; // Ziehe hier die Kamera rein
    public Vector3 positionOffset; // Individuelle Position für jedes Item
    public Vector3 rotationOffset; // Individuelle Rotation für jedes Item

    void Update()
    {
        if (cameraTransform == null) return;

        // Setzt Position relativ zur Kamera
        transform.position = cameraTransform.position + cameraTransform.TransformDirection(positionOffset);

        // Setzt Rotation relativ zur Kamera
        transform.rotation = cameraTransform.rotation * Quaternion.Euler(rotationOffset);
    }
}

