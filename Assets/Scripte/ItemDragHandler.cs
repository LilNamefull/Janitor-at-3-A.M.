using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private InventorySlot originalSlot;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Speichere den Slot
        originalSlot = GetComponentInParent<InventorySlot>();
        originalParent = transform.parent;

        // Löse das Icon aus seinem Parent, damit wir es \"frei\" bewegen können
        transform.SetParent(transform.root);

        // Raycasts ignorieren, damit wir Slots \"darunter\" treffen können
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Icon folgt der Maus
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Zurück zum ursprünglichen Parent, falls wir es nirgendwo ablegen
        transform.SetParent(originalParent);
        canvasGroup.blocksRaycasts = true;

        // Prüfe, ob wir über einem anderen Slot sind
        if (eventData.pointerEnter != null)
        {
            HotbarSlot hotbarSlot = eventData.pointerEnter.GetComponent<HotbarSlot>();
            if (hotbarSlot != null)
            {
                // Übertrage das Item in den Hotbar-Slot
                InventoryItem item = originalSlot.GetItem();
                hotbarSlot.SetItem(item);

                // Inventar-Slot leeren
                originalSlot.ClearSlot();
            }
        }
    }
}
