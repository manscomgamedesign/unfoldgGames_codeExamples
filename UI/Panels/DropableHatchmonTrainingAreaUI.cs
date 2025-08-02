using UnityEngine;
using UnityEngine.EventSystems;

public class DropableHatchmonTrainingAreaUI : DropableItemUI
{
    public TrainingEditDeckPanel m_EditDeckPanel;

    public override void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            dragItem = eventData.pointerDrag.GetComponent<DragableItemUI>();
            dragTargetItem = transform.GetComponentInChildren<DragableItemUI>();

            // target slot is empty, just drop it
            // target slot means the new training Hatchmon Deck slot
            if(dragTargetItem == null &&
                SlotType == ESlotUIType.DECK) // <- no hatchmon ui was assigned in this slot before
            {
                DropOnEmptySlot();

                // simple force swap both slot
                if(transform != null && transform.childCount > 0) transform.GetChild(0).transform.SetParent(dragItem.ParentBeforeDrag);

                // assign the event to turn on the standby drop panel
                if (dragItem.OnDragBeginAction == null) dragItem.OnDragBeginAction += m_EditDeckPanel.TurnOnSTandbyDropSelection; 
                if(dragItem.OnDragEndAction == null) dragItem.OnDragEndAction += m_EditDeckPanel.TurnOffSTandbyDropSelection;    

                // only when the item was came from the stand by slot need to hide the stand by slot
                if(!dragItem.ParentBeforeDrag.TryGetComponent<DropableItemUI>(out var dropComponent))
                {
                    dragItem.ParentBeforeDrag.gameObject.SetActive(false);
                }

                m_EditDeckPanel.TurnOffSTandbyDropSelection();
                m_EditDeckPanel.TurnOnApplyEditButton();
            }

            // is not empty, just switch both slot
            else if(dragTargetItem != null
                 && SlotType == ESlotUIType.DECK)
            {
                SwapSlot();

                // assign the event to turn on the standby drop panel
                if (dragItem.OnDragBeginAction == null)
                    dragItem.OnDragBeginAction += m_EditDeckPanel.TurnOnSTandbyDropSelection;

                // only when the item go back to the stand by slot doesn't require the event anymore
                if (!dragItem.ParentBeforeDrag.TryGetComponent<DropableItemUI>(out var dropComponent))
                {
                    // item back to the standby slot doesn't need the event
                    if (dragTargetItem.OnDragBeginAction != null)
                        dragTargetItem.OnDragBeginAction -= m_EditDeckPanel.TurnOnSTandbyDropSelection;
                }

                m_EditDeckPanel.TurnOffSTandbyDropSelection();
                m_EditDeckPanel.TurnOnApplyEditButton();
            }

            // this is dropping on the standby selection
            else if (SlotType == ESlotUIType.STANDBYSECTIONAREA)
            {
                //unsub events
                if (dragItem.OnDragBeginAction != null)
                    dragItem.OnDragBeginAction -= m_EditDeckPanel.TurnOnSTandbyDropSelection;
                
                if(dragItem.OnDragEndAction != null)
                    dragItem.OnDragBeginAction -= m_EditDeckPanel.TurnOffSTandbyDropSelection;

                //transform.GetChild(0).transform.SetParent(dragItem.parentBeforeDrag);
                m_EditDeckPanel.TurnOffSTandbyDropSelection();

                // assign the hatchmon ui to any avaible standby empty slot
                dragItem.ParentAfterDrag = m_EditDeckPanel.FindEmptyStandbySlot();

                // simple force swap both slot
                dragItem.ParentAfterDrag.GetChild(0).SetParent(dragItem.ParentBeforeDrag);
                dragItem.ParentBeforeDrag.GetChild(0).gameObject.SetActive(false);

                //Debug.Log("Target " + transform.GetChild(0).name);
                m_EditDeckPanel.TurnOnApplyEditButton();
            }

            return;
        }

        base.OnDrop(eventData);
    }
}
