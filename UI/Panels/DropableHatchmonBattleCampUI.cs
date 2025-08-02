using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

/// <summary>
/// handle when hatchmon beening drop into ui slot
/// </summary>
public class DropableHatchmonBattleCampUI : DropableItemUI
{
    public BattleFieldAreaPanel m_BattleFieldAreaPanel { get;  set; }
    private StarLevelDownGradeConfirmPoppu confirmWindow;
    
    public override void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            dragItem = eventData.pointerDrag.GetComponent<DragableItemUI>();
            dragTargetItem = transform.GetComponentInChildren<DragableItemUI>();

            // nothing can do if reach to 24hour and have to reset stage
            if (string.Equals(BattleFieldAreaDataManager.Instance.GetCurrentRunningTime(), "24:00:00"))
            {
                m_BattleFieldAreaPanel.ShowResetStageConfirmWindowPopppu(EPoppuWindowType.Area_BattleField_StageLocked);
                return;
            }

            // means dropping on the same item, then no need to do anything, just place it on the same slot
            if (dragTargetItem == null || dragItem == null) return;
            
            // this is a upgrade book
            if (eventData.pointerDrag.GetComponent<HatchmonInfoUI>() == null)
            {
                var bookDragObj = eventData.pointerDrag;

                // find the upgrade book level
                var levelHoler = bookDragObj.transform.Find("UpgradeLevel");
                int upgradeBookLevel = 0;
                for (int i = 0; i < levelHoler.childCount; i++)
                {
                    if (levelHoler.GetChild(i).gameObject.activeSelf)
                    {
                        upgradeBookLevel = i + 1;
                        break;
                    }
                }
                // get the hatchmon star level
                var hatchmonInfo = dragTargetItem.GetComponent<HatchmonInfoUI>();
                var hatchmonLevel = hatchmonInfo.GetStarLevel();

                // Upgrade!!
                // Star Grade System
                // rule to define some book level can only upgrade specific hatchmon level
                switch (upgradeBookLevel)
                {
                    case 1:
                        if (hatchmonLevel > 2) return;
                        if (hatchmonLevel == 1 || hatchmonLevel == 2)
                            hatchmonLevel++;
                            break;
                    case 2:
                        if (hatchmonLevel > 3) return;
                        if (hatchmonLevel == 1 || hatchmonLevel == 2)                        
                            hatchmonLevel = 3;
                        else if (hatchmonLevel == 3)
                            hatchmonLevel = 4;
                        break;
                    case 3:
                        if (hatchmonLevel > 5) return;
                        if (hatchmonLevel == 1 || hatchmonLevel == 2 || hatchmonLevel == 3)
                            hatchmonLevel = 4;
                        else if (hatchmonLevel == 4)
                            hatchmonLevel = 5;
                        else if (hatchmonLevel == 5)
                            hatchmonLevel = 6;
                        break;
                    case 4:
                        if (hatchmonLevel > 6) return;
                        if (hatchmonLevel == 1 || hatchmonLevel == 2 || hatchmonLevel == 3)
                            hatchmonLevel = 4;
                        else if (hatchmonLevel == 4 || hatchmonLevel == 5)
                            hatchmonLevel = 6;
                        else if (hatchmonLevel == 6)
                            hatchmonLevel = 7;
                        break;
                }
                hatchmonInfo.UpdateStarUI(hatchmonLevel);

                // spawn the star upgrade effect on the upgraded hatchmon slot
                VFXManager.Instance.SpawnEffectOnLocation("Effect_StarLevelUpgrade", transform);

                // if got a bad request from the server, use this level to revert the book
                var itemRevertUpgradeLevel = bookDragObj.GetComponent<HatchmonStarLevelUpgradeItemUI>().GetLevel();

                // check if has the next avaiable book (more than five books in the inventory)
                var bookInventoryData = BattleFieldAreaDataManager.Instance.Cache_Area_Battlefield.bookInventory;

                // more than five books, then swap the used and new book
                if (bookInventoryData.Count > 5)
                {
                    var levelStr = bookInventoryData[5].bookId;
                    var level = int.Parse(levelStr.Substring(levelStr.Length - 1));
                    bookDragObj.GetComponent<HatchmonStarLevelUpgradeItemUI>()
                        .UpdateLevelUI(level);

                    // remove the cache data
                    bookInventoryData[dragItem.DragIndex] = bookInventoryData[5];
                    bookInventoryData.RemoveAt(5);
                }
                // it no, leave the content and data empty, it will let the system show an empty slot
                else
                {
                    bookDragObj.transform.SetParent(dragItem.ParentAfterDrag);
                    bookDragObj.transform.localPosition = Vector3.zero;
                    bookDragObj.GetComponent<DragableItemUI>().enabled = false; // <- can not be drag
                    bookDragObj.SetActive(false);

                    bookInventoryData[dragItem.DragIndex].bookId = string.Empty; // <- means use a empty slot next time
                }

                // just tell the server what happen in this upgrade action
                ServerAPIManager.Instance.CallBATTLEFIRLDStarUpgradeFunction(
                    BattleFieldAreaDataManager.Instance.GetCampIndexByHatchmonId(dragTargetItem.name),
                    dragItem.DragIndex,
                    // Upgrade!!!
                    onSuccess: () =>
                    {
                    },
                    // revert the previous result if bad request
                    onError: () =>
                    {
                    });

                //// *** Things to do
                //// update the upgrade item database after the book used
                //var objectUUID = bookDragObj.GetComponent<UUIDIndentifer>().uuid;
                //var dataList = BattleFieldAreaDataManager.Instance.GetRuntimeData().upgradeItems;
                //var objectToRemove = dataList.FirstOrDefault(o => Guid.Parse(o.uuid) == objectUUID);

                //// delete the data from the database
                //// and sync the data in the battlefield as well
                //if (objectToRemove != null)
                //{
                //    // get the location of the remove object and set this location to the next avaible obeject
                //    var removeObjectLocationIndex = objectToRemove.locationIndex;

                //    // get the remove object index and ready to insert the next objet if exist
                //    var indexTo = dataList.IndexOf(objectToRemove);

                //    // delete from the data
                //    dataList.Remove(objectToRemove);

                //    // check the database if has more books then need to insert the new books on the empty slot.
                //    // the idea is because books location can not excess location index of 10, if yes that means we have a book that is hiding in the list then we can use that to insert to the emtpy slot
                //    // on the UI, becuase the book image was once design to have all the upgrade level images, so can just replace the display data to get the next update book instead destroy the whole gameObject for performance
                //    var nextAvaiableUpgradeItem = dataList.FirstOrDefault(i => i.locationIndex > 10);
                //    if (nextAvaiableUpgradeItem != null)
                //    {
                //        nextAvaiableUpgradeItem.locationIndex = removeObjectLocationIndex;

                //        bookDragObj.name = nextAvaiableUpgradeItem.nameId;

                //        bookDragObj.GetComponent<HatchmonStarLevelUpgradeItemUI>()
                //            .UpdateLevelUI(nextAvaiableUpgradeItem.level);

                //        bookDragObj.GetComponent<UUIDIndentifer>().uuid = Guid.Parse(nextAvaiableUpgradeItem.uuid);

                //        // sync the data to the showing slots list
                //        // basiclly is like the slot list showing on the UI is following the first five objects in the database data
                //        // so let slot index 0 = database data index 0, how the slot shows the items is from top to bottom like index of 0 -> 1 -> 2 -> 3 -> 4 -> 5
                //        // so base on this strcutre then also have to swap the order from the delete object and the next avaiable object\
                //        var indexFrom = dataList.IndexOf(nextAvaiableUpgradeItem);

                //        // swap the index here
                //        dataList.RemoveAt(indexFrom);
                //        dataList.Insert(indexTo, nextAvaiableUpgradeItem);
                //    }
                //    else
                //    {
                //        // finally delete/disable the object in the scene?
                //        // ***FIXING might consider use ObjectPool
                //        //Destroy(bookDragObj);
                //        bookDragObj.transform.SetParent(dragItem.ParentAfterDrag);
                //        bookDragObj.transform.localPosition = Vector3.zero;
                //        bookDragObj.GetComponent<DragableItemUI>().enabled = false; // <- can not be drag
                //        bookDragObj.SetActive(false);
                //    }
                //    // update the UUID database data
                //    IDDataManager.Instance.RemoveId(objectUUID);
                //}
                return;
            }

            // if just switch the hatchmons on the battle camp slot (not stand by slot), then  just switch both the hatchmon
            if(dragItem.ParentBeforeDrag.TryGetComponent<DropableItemUI>(out var dropCom))
            {
                if (dropCom.SlotType == ESlotUIType.DECK)
                {
                    SwapSlot();
                    return;
                }
            }

            // before switch if the star level on the target is higher than 1, we have to warn the user the star level will be drop back to init if confirm to switch
            var targetInfo = dragTargetItem.GetComponent<HatchmonInfoUI>();
            if (targetInfo.GetStarLevel() > 1)
            {
                // show confirm panel
                var window = UIManager.Instance.GetPopupWindowByName(EPoppuWindowType.Area_BattleField_StarLevelDownGrade);

                confirmWindow = window.GetComponent<StarLevelDownGradeConfirmPoppu>();

                // get compare hatchmon ui data
                var mine = dragItem.GetComponent<HatchmonInfoUI>();
                var target = dragTargetItem.GetComponent<HatchmonInfoUI>();

                confirmWindow.UpdateSlotsUI(target, mine);

                window.SetActive(true);
                confirmWindow.OnConfirmButtonClicked += SwapFromStandBySlot;
                confirmWindow.OnGoBackButtonClicked += OnConfirmPopuPanelBackButtonClick; // add event handler for the go back button
            }
            // if both hatchmon ui are just star level 1, then just swap them
            else
            {
                SwapFromStandBySlot();
            }
            return;
        }
        base.OnDrop(eventData);
    }

    protected void SwapFromStandBySlot()
    {
        SwapSlot();

        //// drag and drop rule to set/switch
        //// the one on the battle camp now can not be drag
        //// the one on the stand by now can be drag
        //dragItem.enabled = false; // <- the one goes to the battlecamp slot
        //dragTargetItem.enabled = true; // <- the one goes to the standby slot

        // update the star level
        // the one on the battle camp slot is star level1
        // the one back to the stand by has no star level
        var mine = dragItem.GetComponent<HatchmonInfoUI>();
        mine.UpdateStarUI(1);

        var target = dragTargetItem.GetComponent<HatchmonInfoUI>();
        target.UpdateStarUI(0);

        if (confirmWindow == null) return;

        // make sure to cancel the event when finish
        confirmWindow.OnConfirmButtonClicked -= SwapFromStandBySlot;
        confirmWindow.OnGoBackButtonClicked -= OnConfirmPopuPanelBackButtonClick;
    }

    private void OnConfirmPopuPanelBackButtonClick()
    {
        if (confirmWindow == null) return;

        // make sure to cancel the event when finish
        confirmWindow.OnConfirmButtonClicked -= SwapFromStandBySlot;
        confirmWindow.OnGoBackButtonClicked -= OnConfirmPopuPanelBackButtonClick;
    }
}
