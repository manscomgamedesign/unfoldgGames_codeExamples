using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TrainingEditDeckPanel : MonoBehaviour
{
    [SerializeField] private TrainingAreaPanel m_TrainingAreaPanel;

    [SerializeField] private GameObject trainingAreaInfoPanel;

    [SerializeField] private GameObject trainingHatchmonDeck_Slot_Prefab;
    [SerializeField] private GameObject standByHitchmon_slot_Prefab;
    [SerializeField] private GameObject hitchmon_DragableSlot_Prefab;

    [SerializeField] private SSkinUIData trainingHatchmonDeck_SlotSkinData;
    [SerializeField] private SSkinUIData standByHitchmon_SlotSkinData;

    [SerializeField] private Transform deck_GridContent;
    [SerializeField] private Transform standBy_GridContent;

    /// when hatmon ui drop from the new deck to this select, hatchmon UI will be auto assign to one of the standy by empty slot
    [SerializeField] private GameObject standByDroppableSelection;

    [SerializeField] private Button back_button;
    [SerializeField] private Button applyDeckEdit_Button;

    [SerializeField] private Sprite buttonDeactiveSprite;
    [SerializeField] private Sprite buttonActiveSprite;

    [SerializeField] private ScrollRect m_ScrollRect;

    // cache data for getting back the previous setup battle camp hatchmons if the user doesn't want to apply the data
    [SerializeField] private Transform[] preiousHatchmonsOnTrainingDeck = new Transform[5];

    private Transform[] allTrainingHatchingDeckSlots;
    private readonly int maxDeckSlotCount = 5; // <- fixed 5 slot allow in the battle camp

    private List<Transform> allstandByHitchmonSlots = new();
    private int standByHatchmon_MaxCount = 15; // <- use to pre create slots (at least how many should show on the screen even emtpy) on the grid and see if need extra slot when more data add in
    private int standByHatchmon_Count; // <- extra slot that need to create

    private List<HatchmonData> allStandByHitchmonData = new();

    /// <summary>
    /// just to record the changed battle camp hatchmon hatchId to update the database
    /// </summary>
    private string[] campChangeLog = new string[5];

    //[SerializeField] private int[] preiousHatchmonsOnTrainingHatchmonDeckStarLevel = new int[5];

    //private bool isDropEventRegistered = false;

    /// when panel open and things need to be reset
    private void OnEnable()
    {
        m_ScrollRect.verticalNormalizedPosition = 1f;
    }

    private void OnDisable()
    {
        // turn off the apply
        applyDeckEdit_Button.interactable = false;
        applyDeckEdit_Button.GetComponent<Image>().sprite = buttonDeactiveSprite;
    }

    private void Awake()
    {
        PreInitDeckSlots();

        PreInitStandbyHatchmonSlots();

        back_button.onClick.AddListener(() => {
            ToSwapDeckSlotsNoApplyClick();
            gameObject.SetActive(false);
            trainingAreaInfoPanel.SetActive(true);
            m_TrainingAreaPanel.StartRewardTimer();
        });

        applyDeckEdit_Button.onClick.AddListener(() =>
        {
            //UpdateDataToDatabase();
            OnApplyEditButtonClick();        
        });

        // can be drop on this area, but only open this select when dragging from the deck slot
        var dropComponent = standByDroppableSelection.AddComponent<DropableHatchmonTrainingAreaUI>();
        dropComponent.m_EditDeckPanel = this;
        dropComponent.SlotType = ESlotUIType.STANDBYSECTIONAREA;

        standByDroppableSelection.SetActive(false);

        // confirm the grid fit to all screen size
        standBy_GridContent.gameObject.AddComponent<GridLayoutGroupCellsScreenResizing>().ResizeCells();
    }

    private void PreInitDeckSlots()
    {
        allTrainingHatchingDeckSlots = new Transform[maxDeckSlotCount];

        // pre generate 10 slots for ready-use
        for (int i = 0; i < allTrainingHatchingDeckSlots.Length; i++)
        {
            GameObject slot = Instantiate(trainingHatchmonDeck_Slot_Prefab, deck_GridContent);
            slot.GetComponent<RectTransform>().localPosition = Vector3.zero;

            // is will add the needed image component directly
            SkinUISlot skin = slot.AddComponent<SkinUISlot>();
            skin.enabled = true;
            skin.skinData = trainingHatchmonDeck_SlotSkinData;

            //// this slot has star level
            //slot.AddComponent<TrainingHatchmonDeckSlotUI>();

            // can be drop
            var dropComponent = slot.AddComponent<DropableHatchmonTrainingAreaUI>();
            dropComponent.m_EditDeckPanel = this;
            dropComponent.SlotType = ESlotUIType.DECK;

            // can be click
            slot.AddComponent<ClickableItem>();

            //allTrainingHatchingDeckSlots.Add(slot.AddComponent<MercenarySummonSlot>());
            allTrainingHatchingDeckSlots[i] = slot.transform;
        }
    }

    private void PreInitStandbyHatchmonSlots()
    {
        for (int i = 0; i < standByHatchmon_MaxCount; i++)
        {
            CreateStandByHatchmonSlot();
        }

        // resize the cells to fit all device screen size
        //standByHatchmon_Grid_Content_Group.gameObject.AddComponent<GridLayoutGroupCellsScreenResizing>().ResizeCells();
    }

    private void CreateStandByHatchmonSlot()
    {
        GameObject slot = Instantiate(standByHitchmon_slot_Prefab, standBy_GridContent);
        slot.GetComponent<RectTransform>().localPosition = Vector3.zero;

        // is will add the needed image component directly
        SkinUISlot skin = slot.AddComponent<SkinUISlot>();
        skin.enabled = true;
        skin.skinData = standByHitchmon_SlotSkinData;

        // can be drop
        //slot.AddComponent<DropableItemUI>().enabled = false;

        // can be click
        slot.AddComponent<ClickableItem>().enabled = false;

        allstandByHitchmonSlots.Add(slot.transform);

        var hatchmonUIObj = Instantiate(hitchmon_DragableSlot_Prefab);
        hatchmonUIObj.transform.SetParent(slot.transform);
        hatchmonUIObj.transform.localPosition = Vector3.zero;
        hatchmonUIObj.transform.localScale = Vector3.one;

        // can update display infos
        hatchmonUIObj.AddComponent<HatchmonInfoUI>();

        // can be drag
        hatchmonUIObj.AddComponent<DragableItemUI>();

        slot.SetActive(false);
    }

    /// <summary>
    /// grab and swap all the hatchmons ui object data from the previous panel to the current panel
    /// use this for data cacheing and show what happen whenever this panel opens
    /// </summary>
    public void FromSwapHatchmonDeckSlots()
    {
        foreach (var slot in allTrainingHatchingDeckSlots)
        {
            foreach (Transform item in slot)
            {
                Destroy(item.gameObject);
            }
        }

        // whatever hatchmons was assigned on the battleFieldPanel, just move the hatchmon objects to this panel
        var allFromHatchmonDeckSlots = m_TrainingAreaPanel.GetAllHatchmonDeckSlots();
        for (int i = 0; i < allTrainingHatchingDeckSlots.Length; i++)
        {
            if (allFromHatchmonDeckSlots[i].childCount <= 0) continue; // slot can be empty
            var hatchmonObj = allFromHatchmonDeckSlots[i].GetChild(0);
            hatchmonObj.SetParent(allTrainingHatchingDeckSlots[i]);
            hatchmonObj.localPosition = Vector3.zero;

            // can be drag
            var dragableItemUI = hatchmonObj.GetComponent<DragableItemUI>();
            dragableItemUI.enabled = true;

            // check if this is hatchmon ui
            if (!string.IsNullOrEmpty(hatchmonObj.GetComponent<HatchmonInfoUI>().HatchId))
            {
                // assign the event to turn on the standby drop panel
                if (dragableItemUI.OnDragBeginAction == null)
                {
                    dragableItemUI.OnDragBeginAction += TurnOnSTandbyDropSelection;
                }
                if (dragableItemUI.OnDragEndAction == null) dragableItemUI.OnDragEndAction += TurnOffSTandbyDropSelection;
            }

            preiousHatchmonsOnTrainingDeck[i] = hatchmonObj;
        }
    }

    /// <summary>
    /// because no apply button clicked, given up all the changes made on the deck
    /// so just swap them back to the previous panel
    /// </summary>
    public void ToSwapDeckSlotsNoApplyClick()
    {
        // check every hatchmons obj on the current slot
        // store them into an temp array
        // swap them back!
        var allFromHatchmonDeckSlots = m_TrainingAreaPanel.GetAllHatchmonDeckSlots();

        for (int i = 0; i < allTrainingHatchingDeckSlots.Length; i++)
        {
            var hatchmonObj = preiousHatchmonsOnTrainingDeck[i];
            if (hatchmonObj == null) continue; // slot can be empty
            hatchmonObj.SetParent(allFromHatchmonDeckSlots[i]);
            hatchmonObj.localPosition = Vector3.zero;

            // can not be drag
            var dragableItemUI = hatchmonObj.GetComponent<DragableItemUI>();
            dragableItemUI.enabled = false;

            // check if this is hatchmon ui
            if (!string.IsNullOrEmpty(hatchmonObj.GetComponent<HatchmonInfoUI>().HatchId))
            {
                // Unassign the event to turn on the standby drop panel
                if (dragableItemUI.OnDragBeginAction != null)
                {
                    Debug.Log("UnASSIGN");
                    dragableItemUI.OnDragBeginAction -= TurnOnSTandbyDropSelection;
                }
                if (dragableItemUI.OnDragEndAction != null) dragableItemUI.OnDragEndAction -= TurnOffSTandbyDropSelection;
            }
        }
    }

    /// <summary>
    /// grab all the changed hatchmons ui object (which user drag and drop edit the slot on this panel)
    /// so apply the swap changes to the preious panel
    /// </summary>
    public void ToSwapDeckSlotsWithApplyClicked()
    {
        // check every hatchmons obj on the current slot
        // store them into an temp array
        // swap them back!
        var allFromHatchmonDeckSlots = m_TrainingAreaPanel.GetAllHatchmonDeckSlots();

        for (int i = 0; i < allTrainingHatchingDeckSlots.Length; i++)
        {
            if (allTrainingHatchingDeckSlots[i].childCount <= 0) continue; // slot can be empty
            var hatchmonObj = allTrainingHatchingDeckSlots[i].GetChild(0);
            hatchmonObj.SetParent(allFromHatchmonDeckSlots[i]);
            hatchmonObj.localPosition = Vector3.zero;

            // can not be drag
            var dragableItemUI = hatchmonObj.GetComponent<DragableItemUI>();
            dragableItemUI.enabled = false;

            // check if this is hatchmon ui
            if (!string.IsNullOrEmpty(hatchmonObj.GetComponent<HatchmonInfoUI>().HatchId))
            {
                // Unassign the event to turn on the standby drop panel
                if (dragableItemUI.OnDragBeginAction != null)
                {
                    dragableItemUI.OnDragBeginAction -= TurnOnSTandbyDropSelection;
                }
                if (dragableItemUI.OnDragEndAction != null) dragableItemUI.OnDragEndAction -= TurnOffSTandbyDropSelection;
            }
        }
    }

    public void OpenPanel()
    {
        AssignStandbyHatchmonsOnSlot();

        FromSwapHatchmonDeckSlots();

        // close and show panels
        gameObject.SetActive(true);
        trainingAreaInfoPanel.SetActive(false);
    }

    private void AssignStandbyHatchmonsOnSlot()
    {
        // pre action
        allStandByHitchmonData.Clear();
        foreach (var slot in allstandByHitchmonSlots)
            slot.gameObject.SetActive(false);

        // find the hatchmons
        var allHatchmonsData = HatchmonDataManager.Instance.Cache_InGame_HatchmonInventory;
        var compareArea = EAreaType.HATCHMON_INVENTORY.ToString();

        foreach (var hatchmonData in allHatchmonsData)
        {
            if (string.Equals(hatchmonData.Value.placedArea, compareArea)) // <- means the hatchmon should be on the stand by 
            {
                allStandByHitchmonData.Add(hatchmonData.Value);
            }
        }

        // assign the data
        foreach (var hatchmonData in allStandByHitchmonData)
        {
            //var hatchmonData = hatchmon.hatchmonStatsData; // <- in-game data
            var hatchmonProperty = HatchmonDataManager.Instance.GetHatchmonPropertyByHatchmonId(hatchmonData.hatchmonId);

            var slot = GetAvaiableStandbySlot(); // <- assign to this emtpy slot

            var hatchmonUIObj = slot.GetChild(0);
            hatchmonUIObj.name = hatchmonData.hatchmonId; // <- override the name

            // assign the data on the ui
            hatchmonUIObj.GetComponent<HatchmonInfoUI>()
                .SetData(
                    hatchmonData.hatchmonId,
                    hatchmonProperty.grade,
                    hatchmonProperty.creatureType,
                    hatchmonData.inGame_Level,
                    0,  // <- means the star level will be hide
                    hatchmonData.hatchId);

            slot.gameObject.SetActive(true);

            hatchmonUIObj.transform.localPosition = Vector3.zero;
            hatchmonUIObj.gameObject.SetActive(true);

            // can be drop
            //slot.GetComponent<DropableItemUI>().enabled = true;

            // can be click
            slot.GetComponent<ClickableItem>().enabled = true;
        }
    }

    private Transform GetAvaiableStandbySlot()
    {
        for (int i = 0; i < allstandByHitchmonSlots.Count; i++)
        {
            var slot = allstandByHitchmonSlots[i];

            if (slot.childCount > 0 && !slot.gameObject.activeSelf)
                return slot;
        }

        // if need more space
        CreateStandByHatchmonSlot();
        return GetAvaiableStandbySlot();
    }

    //private void OnAllHatchmonsDatabaseDataRecieved(List<HatchmonDataBaseData> callback)
    //{
    //    var allHatchmons = callback;

    //    // 4. whoever hatchmon has not assign to any areas then we can use it for battlefied
    //    // ***FIXING Cached might use a cache techs here to compare the cache data and the database data to avoid continuely Instantiate and Destroy and UI to optimize the performance 
    //    // for now I just destroy the object for fast result
    //    foreach (var slot in allstandByHitchmonSlots)
    //    {
    //        foreach (Transform obj in slot)
    //        {
    //            obj.parent = null;
    //            Destroy(obj.gameObject);
    //        }
    //        slot.gameObject.SetActive(false);
    //    }

    //    var allStandByHitchmonData = allHatchmons.Where(o => o.hatchmonStatsData.occupyArea == EAreaType.NULL).ToList();
    //    if (allStandByHitchmonData.Count > standByHatchmon_MaxCount)
    //    {
    //        standByHatchmon_MaxCount = allStandByHitchmonData.Count;
    //        standByHatchmon_Count = allStandByHitchmonData.Count - standByHatchmon_Count;
    //        for (int i = 0; i < standByHatchmon_Count; i++)
    //        {
    //            CreateStandByHatchmonSlot();
    //        }
    //    }

    //    foreach (var hatchmon in allStandByHitchmonData)
    //    {
    //        var hatchmonData = hatchmon.hatchmonStatsData; // <- in-game data
    //        var hatchmonProperty = HatchmonDataManager.Instance.GetHatchmonPropertyByNameId(hatchmonData.id);

    //        var slot = FindEmptyStandbySlot(); // <- assign to this emtpy slot

    //        var hatchmonUIObj = Instantiate(hitchmon_DragableSlot_Prefab);
    //        hatchmonUIObj.name = hatchmonData.id;

    //        // assign the data on the ui
    //        hatchmonUIObj.AddComponent<DragableHatchmonUI>()
    //        .OnUpdateUI(
    //            hatchmonData.id,
    //            hatchmonProperty.grade,
    //            hatchmonProperty.creatureType,
    //            hatchmonData.level,
    //            0); // <- means the star level will be hide

    //        hatchmonUIObj.transform.SetParent(slot);
    //        hatchmonUIObj.transform.localPosition = Vector3.zero;
    //        hatchmonUIObj.transform.localScale = Vector3.one;

    //        // can be drag
    //        hatchmonUIObj.AddComponent<DragableItemUI>();

    //        // can be drop
    //        //slot.GetComponent<DropableItemUI>().enabled = true;

    //        // can be click
    //        slot.GetComponent<ClickableItem>().enabled = true;
    //    }
    //}

    //private void UpdateDataToDatabase()
    //{
    //    // update the battlefield runtime data and get ready to push the data back to the database when then whole panel exist
    //    var runtimeData = TrainingAreaDataManager.Instance.GetRuntimeData();

    //    for (int i = 0; i < allTrainingHatchingDeckSlots.Length; i++)
    //    {
    //        // slot can be emtpy, but remember also clear the data, if the hatchmon was drag out from the deck
    //        if (allTrainingHatchingDeckSlots[i].childCount <= 0)
    //        {
    //            runtimeData.hatchmonsData[i].id = string.Empty;
    //            runtimeData.hatchmonsData[i].locationIndex = 0;
    //            continue; 
    //        }

    //        // slot is not empty
    //        var hatchmonObj = allTrainingHatchingDeckSlots[i].GetChild(0);

    //        // update the runtime data
    //        runtimeData.hatchmonsData[i].id = hatchmonObj.name;
    //        runtimeData.hatchmonsData[i].locationIndex = i + 1;
    //        //runtimeData.hatchmonsData[i].starLevel = hatchmonObj.GetComponent<DragableHatchmonUI>().GetStarLevel();

    //        // upload the hatchmon database to who is really on battlefield area
    //        // whoeveer is on the battle camp change the occupyArea to BattleFieldArea
    //        HatchmonDataManager.Instance.GetSinglehatchmonRuntimeStateDataWithId(hatchmonObj.name)
    //        .occupyArea = EAreaType.TRAININGAREA;
    //    }

    //    // what data needs to be reset for all the standby hatchmons
    //    foreach (var slots in allstandByHitchmonSlots)
    //    {
    //        if (slots.childCount > 0)
    //        {
    //            // then the rest in the standby change the occupyArea to null
    //            HatchmonDataManager.Instance.GetSinglehatchmonRuntimeStateDataWithId(slots.GetChild(0).name)
    //                .occupyArea = EAreaType.NULL;
    //        }
    //    }
    //    HatchmonDataManager.Instance.SendRunTimeDataToDatabase();
    //    TrainingAreaDataManager.Instance.SendRunTimeDataToDatabase();
    //}

    public Transform FindEmptyStandbySlot()
    {
        foreach (Transform slot in allstandByHitchmonSlots)
        {
            if (!slot.gameObject.activeSelf)
            {
                slot.gameObject.SetActive(true);
                return slot;
            }
        }
        CreateStandByHatchmonSlot(); // no enough slot, just create one more
        return FindEmptyStandbySlot();
    }

    private void OnApplyEditButtonClick()
    {
        for (int i = 0; i < allTrainingHatchingDeckSlots.Length; i++)
        {
            if (!allTrainingHatchingDeckSlots[i].gameObject.activeSelf) continue;

            var hatchmonObj = allTrainingHatchingDeckSlots[i].GetChild(0);
            
            // check if this is hatchmon ui
            if (!string.IsNullOrEmpty(hatchmonObj.GetComponent<HatchmonInfoUI>().HatchId))
            { 
                var hatchmonData = hatchmonObj.GetComponent<HatchmonInfoUI>();
                campChangeLog[i] = hatchmonData.HatchId;
            }
            else // <- not number means this is an empty slot
            {
                // if empty is ok, send an empty data 
                campChangeLog[i] = "EMPTY"; // <- the string rule of setting the an empty slot on the server
            }
        }

        // update database
        ServerAPIManager.Instance.CallTRAININGCampChangeFunction(
            campChangeLog[0],
            campChangeLog[1],
            campChangeLog[2],
            campChangeLog[3],
            campChangeLog[4],
            () => {
                ToSwapDeckSlotsWithApplyClicked();
                m_TrainingAreaPanel.UpdatePowerCountUI();

                gameObject.SetActive(false);
                trainingAreaInfoPanel.SetActive(true);
                m_TrainingAreaPanel.StartRewardTimer();
            });
    }

        public void TurnOnApplyEditButton()
    {
        if (applyDeckEdit_Button.interactable) return;

        applyDeckEdit_Button.interactable = true; // can apply now
        applyDeckEdit_Button.GetComponent<Image>().sprite = buttonActiveSprite;
    }

    public void TurnOnSTandbyDropSelection()
    {
        standByDroppableSelection.SetActive(true);
    }

    public void TurnOffSTandbyDropSelection()
    {
        standByDroppableSelection.SetActive(false);
    }
}
