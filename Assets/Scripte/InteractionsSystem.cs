using UnityEngine;
using TMPro;

public class InteractionsSystem : MonoBehaviour
{
    public Transform playerCamera;
    public float interactionRange = 3f;
    public LayerMask interactableLayer;
    public TextMeshProUGUI interactionText; // UI-Text für Hinweise

    void Update()
    {
        
       
        CheckForInteractable();
    }

    void CheckForInteractable()
    {
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionRange, interactableLayer))
        {
            Interactable interactable = hit.collider.GetComponent<Interactable>();
            if (interactable != null)
            {
                interactionText.text = "[E] " + interactable.promptMessage;
                interactionText.enabled = true;

                if (Input.GetKeyDown(KeyCode.E))
                {
                    interactable.Interact();
                }
            }
        }
        else
        {
            interactionText.enabled = false;
        }
    }
}
