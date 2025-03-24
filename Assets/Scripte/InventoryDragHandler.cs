using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image dragIcon;
    private Transform originalParent;
    private InventoryItem currentItem;

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        currentItem = originalParent.GetComponent<InventorySlot>().GetItem();

        if (currentItem == null) return;

        dragIcon.sprite = currentItem.icon;
        dragIcon.enabled = true;
        transform.SetParent(transform.root);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragIcon.enabled)
        {
            dragIcon.transform.position = Input.mousePosition;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Transform newParent = eventData.pointerEnter?.transform;

        if (newParent != null && newParent.GetComponent<HotbarSlot>() != null)
        {
            newParent.GetComponent<HotbarSlot>().SetItem(currentItem);
        }

        transform.SetParent(originalParent);
        dragIcon.enabled = false;
    }
}
