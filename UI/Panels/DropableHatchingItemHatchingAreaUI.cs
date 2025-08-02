using UnityEngine;
using UnityEngine.EventSystems;

public class DropableHatchingItemHatchingAreaUI : DropableItemUI
{
    public override void OnDrop(PointerEventData eventData)
    {
        dragItem = eventData.pointerDrag.GetComponent<DragableItemUI>();
        //dragTargetItem = transform.GetComponentInChildren<DragableItemUI>();

        // Empty SLot
        // only empty slot can drop the egg
        // if not active means no item, then it is an empty slot
        if (!transform.GetChild(0).gameObject.activeSelf)
        {
            if (dragItem == null) return;

            //swap the empty slot on the back
            dragTargetItem = transform.GetChild(0).GetComponent<DragableItemUI>();
            SwapSlot();

            // when on the deck, can not drag anymore
            dragItem.enabled = false;

            // update the UI
            // reset hatching egg start hatching time -> "00:00:00"
            var hatchingItemInfoUI = dragItem.GetComponent<HatchingItemInfoUI>();
            hatchingItemInfoUI.AssignRunningTimer("00:00:00"); 
            hatchingItemInfoUI.StartRunningTimer(); // <- start couning the timer

            // hide the standby slot
            dragItem.ParentBeforeDrag.gameObject.SetActive(false);
            dragTargetItem.gameObject.SetActive(true);

            // just tell the server what happen in this upgrade action
            ServerAPIManager.Instance.CallHatchingStartRequestFunction(
                transform.GetSiblingIndex(),
                hatchingItemInfoUI.LayId,
                onSuccess: () =>
                {
                },
                // revert the previous result if bad request
                onError: () =>
                {
                    // assign back to the previous slot
                    dragItem.transform.SetParent(dragItem.ParentBeforeDrag);
                    dragItem.transform.localPosition = Vector3.zero;
                    dragItem.transform.localScale = Vector3.one;
                    dragItem.ParentBeforeDrag.gameObject.SetActive(true);

                    // hide the time back
                    var hatchingItemInfoUI = dragItem.GetComponent<HatchingItemInfoUI>();
                    hatchingItemInfoUI.AssignRunningTimer(string.Empty);
                    hatchingItemInfoUI.StopRunningTimer();

                    // can be dragable again
                    dragItem.GetComponent<DragableItemUI>().enabled = true;
                });

            base.OnDrop(eventData);
            return;
        }
        // Has Item
        // when the children (the actual ui item) is setActive true, means the slot is now not empty, it has item
        // then force the item back to the previous slot
        else
        {
            if (dragItem == null) return;
            dragItem.transform.SetParent(dragItem.ParentBeforeDrag);
            return;
        }

        //base.OnDrop(eventData);
    }
}
