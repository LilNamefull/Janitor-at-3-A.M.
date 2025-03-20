using UnityEngine;

public class Interactable : MonoBehaviour
{
    public string promptMessage = "Interagieren"; // Der Text, der angezeigt wird

    public virtual void Interact()
    {
        Debug.Log("Interagiert mit: " + gameObject.name);
    }
}
