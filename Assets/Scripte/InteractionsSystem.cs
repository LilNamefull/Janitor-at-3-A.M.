using UnityEngine;
using TMPro;

public class InteractionsSystem : MonoBehaviour
{
    public Transform playerCamera;
    public float interactionRange = 3f;
    
    public TextMeshProUGUI interactionText; // UI-Text für Hinweise

    void Update()
    {
        CheckForInteractable();

        if (Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
    }

    void CheckForInteractable()
    {
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionRange))
        {
            Interactable interactable = hit.collider.GetComponent<Interactable>();

            if (interactable != null)
            {
                interactionText.text = "[E] " + interactable.promptMessage;
                interactionText.enabled = true;
            }
        }
        else
        {
            interactionText.enabled = false;
        }
    }

    void Interact()
    {
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionRange))
        {
            Interactable interactable = hit.collider.GetComponent<Interactable>();

            if (interactable != null)
            {
                interactable.Interact();
            }
        }
    }
}
