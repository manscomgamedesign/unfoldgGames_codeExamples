using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropableItemUI : MonoBehaviour, IDropHandler
{
    public ESlotUIType SlotType { get; set; }

    [SerializeField] protected DragableItemUI dragItem;
    [SerializeField] protected DragableItemUI dragTargetItem;

    public Action OnDropEvent;

    public virtual void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            if (eventData.pointerDrag.GetComponent<Scrollbar>()) return;
       
            if (!eventData.pointerDrag.TryGetComponent<DragableItemUI>(out var dragItem)) return;

            var dragRect = dragItem.GetComponent<RectTransform>();
            var dropRect = transform.GetComponent<RectTransform>();

            if (dragItem.RectOverlaps(dragRect, dropRect))
            {
                dragItem.transform.position = transform.position;
            }
            dragItem.ParentAfterDrag = transform;
        }
    }

    /// <summary>
    /// Swap both hatchmon UI when both slot meet
    /// </summary>
    protected virtual void SwapSlot()
    {
        if (dragItem == null) return;

        // Switching both slot position
        var mySlot = dragItem.ParentAfterDrag;
        var targetSlot = transform;

        var from = dragItem.transform;
        var to = dragTargetItem.transform;

        from.SetParent(targetSlot);
        from.localPosition = Vector3.zero;

        to.SetParent(mySlot);
        to.localPosition = Vector3.zero;

        dragItem.ParentAfterDrag = targetSlot;

        if (OnDropEvent != null)
            OnDropEvent.Invoke();
    }

    /// <summary>
    /// drop the item on an empty slot
    /// </summary>
    protected virtual void DropOnEmptySlot()
    {
        if (dragItem == null) return;

        var dragRect = dragItem.GetComponent<RectTransform>();
        var dropRect = transform.GetComponent<RectTransform>();

        if (dragItem.RectOverlaps(dragRect, dropRect))
        {
            dragItem.transform.position = transform.position;
        }

        dragItem.ParentAfterDrag = transform;

        if (OnDropEvent != null)
            OnDropEvent.Invoke();
    }
}
