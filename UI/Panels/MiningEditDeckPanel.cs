using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class MiningEditDeckPanel : MonoBehaviour
{
    [SerializeField] private MiningAreaPanel m_MiningAreaPanel;

    [SerializeField] private GameObject miningAreaInfoPanel;

    [SerializeField] private GameObject miningHatchmonDeck_Slot_Prefab;
    [SerializeField] private GameObject standByHitchmon_slot_Prefab;
    [SerializeField] private GameObject hitchmon_DragableSlot_Prefab;

    [SerializeField] private SSkinUIData miningHatchmonDeck_SlotSkinData;
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
    [SerializeField] private Transform[] preiousHatchmonsOnMiningDeck = new Transform[5];

    private Transform[] allMiningHatchingDeckSlots;
    private readonly int maxDeckSlotCount = 5; // <- fixed 5 slot allow in the battle camp

    private List<Transform> allstandByHitchmonSlots = new();
    private int standByHatchmon_MaxCount = 15; // <- use to pre create slots (at least how many should show on the screen even emtpy) on the grid and see if need extra slot when more data add in
    private int standByHatchmon_Count; // <- extra slot that need to create

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
            miningAreaInfoPanel.SetActive(true);
        });

        applyDeckEdit_Button.onClick.AddListener(() =>
        {
            //UpdateDataToDatabase();
            ToSwapDeckSlotsWithApplyClicked();
            gameObject.SetActive(false);
            miningAreaInfoPanel.SetActive(true);
        });

        // can be drop on this area, but only open this select when dragging from the deck slot
        var dropComponent = standByDroppableSelection.AddComponent<DropableHatchmonMiningAreaUI>();
        dropComponent.m_EditDeckPanel = this;
        dropComponent.SlotType = ESlotUIType.STANDBYSECTIONAREA;

        standByDroppableSelection.SetActive(false);

        // confirm the grid fit to all screen size
        standBy_GridContent.gameObject.AddComponent<GridLayoutGroupCellsScreenResizing>().ResizeCells();
    }

    private void PreInitDeckSlots()
    {
        allMiningHatchingDeckSlots = new Transform[maxDeckSlotCount];

        // pre generate 10 slots for ready-use
        for (int i = 0; i < allMiningHatchingDeckSlots.Length; i++)
        {
            GameObject slot = Instantiate(miningHatchmonDeck_Slot_Prefab, deck_GridContent);
            slot.GetComponent<RectTransform>().localPosition = Vector3.zero;

            // is will add the needed image component directly
            SkinUISlot skin = slot.AddComponent<SkinUISlot>();
            skin.enabled = true;
            skin.skinData = miningHatchmonDeck_SlotSkinData;

            //// this slot has star level
            //slot.AddComponent<TrainingHatchmonDeckSlotUI>();

            // can be drop
            var dropComponent = slot.AddComponent<DropableHatchmonMiningAreaUI>();
            dropComponent.m_EditDeckPanel = this;
            dropComponent.SlotType = ESlotUIType.DECK;

            // can be click
            slot.AddComponent<ClickableItem>();

            //allTrainingHatchingDeckSlots.Add(slot.AddComponent<MercenarySummonSlot>());
            allMiningHatchingDeckSlots[i] = slot.transform;
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

        slot.SetActive(false);

        allstandByHitchmonSlots.Add(slot.transform);
    }

    /// <summary>
    /// grab and swap all the hatchmons ui object data from the previous panel to the current panel
    /// use this for data cacheing and show what happen whenever this panel opens
    /// </summary>
    public void FromSwapHatchmonDeckSlots()
    {
        foreach (var slot in allMiningHatchingDeckSlots)
        {
            foreach (Transform item in slot)
            {
                Destroy(item.gameObject);
            }
        }

        // whatever hatchmons was assigned on the battleFieldPanel, just move the hatchmon objects to this panel
        var allFromHatchmonDeckSlots = m_MiningAreaPanel.GetAllHatchmonDeckSlots();
        for (int i = 0; i < allMiningHatchingDeckSlots.Length; i++)
        {
            if (allFromHatchmonDeckSlots[i].childCount <= 0) continue; // slot can be empty
            var hatchmonObj = allFromHatchmonDeckSlots[i].GetChild(0);
            hatchmonObj.SetParent(allMiningHatchingDeckSlots[i]);
            hatchmonObj.localPosition = Vector3.zero;

            // can be drag
            var dragableItemUI = hatchmonObj.GetComponent<DragableItemUI>();
            dragableItemUI.enabled = true;

            // assign the event to turn on the standby drop panel
            if (dragableItemUI.OnDragBeginAction == null)
                dragableItemUI.OnDragBeginAction += TurnOnSTandbyDropSelection;

            preiousHatchmonsOnMiningDeck[i] = hatchmonObj;

            //var ui = hatchmonObj.GetComponent<DragableHatchmonUI>();
            //preiousHatchmonsOnTrainingHatchmonDeckStarLevel[i] = ui.GetStarLevel();
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
        var allFromHatchmonDeckSlots = m_MiningAreaPanel.GetAllHatchmonDeckSlots();

        for (int i = 0; i < allMiningHatchingDeckSlots.Length; i++)
        {
            var hatchmonObj = preiousHatchmonsOnMiningDeck[i];
            if (hatchmonObj == null) continue; // slot can be empty
            hatchmonObj.SetParent(allFromHatchmonDeckSlots[i]);
            hatchmonObj.localPosition = Vector3.zero;

            // can not be drag
            hatchmonObj.GetComponent<DragableItemUI>().enabled = false;

            // assign the star level back
            //var ui = hatchmonObj.GetComponent<DragableHatchmonUI>();
            //ui.UpdateStarUI(preiousHatchmonsOnTrainingHatchmonDeckStarLevel[i]);
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
        var allFromHatchmonDeckSlots = m_MiningAreaPanel.GetAllHatchmonDeckSlots();

        for (int i = 0; i < allMiningHatchingDeckSlots.Length; i++)
        {
            if (allMiningHatchingDeckSlots[i].childCount <= 0) continue; // slot can be empty
            var hatchmonObj = allMiningHatchingDeckSlots[i].GetChild(0);
            hatchmonObj.SetParent(allFromHatchmonDeckSlots[i]);
            hatchmonObj.localPosition = Vector3.zero;

            // can not be drag
            hatchmonObj.GetComponent<DragableItemUI>().enabled = false;
        }
    }

    public void AssignHatchmonsOnStandByGrid(Action onSuccess)
    {
        //// ***FIXING Cache data
        //// 1.get total hatchmons owned by the user from the database
        //HatchmonDataManager.Instance.GetDataFromDatabaseCallBack(
        //    data =>
        //    {
        //        OnAllHatchmonsDatabaseDataRecieved(data);
        //        onSuccess?.Invoke();
        //    });
    }

    private void OnAllHatchmonsDatabaseDataRecieved(List<HatchmonDataBaseData> callback)
    {
        var allHatchmons = callback;

        // 4. whoever hatchmon has not assign to any areas then we can use it for battlefied
        // ***FIXING Cached might use a cache techs here to compare the cache data and the database data to avoid continuely Instantiate and Destroy and UI to optimize the performance 
        // for now I just destroy the object for fast result
        foreach (var slot in allstandByHitchmonSlots)
        {
            foreach (Transform obj in slot)
            {
                obj.parent = null;
                Destroy(obj.gameObject);
            }
            slot.gameObject.SetActive(false);
        }

        var allStandByHitchmonData = allHatchmons.Where(o => o.hatchmonStatsData.occupyArea == EAreaType.NULL).ToList();
        if (allStandByHitchmonData.Count > standByHatchmon_MaxCount)
        {
            standByHatchmon_MaxCount = allStandByHitchmonData.Count;
            standByHatchmon_Count = allStandByHitchmonData.Count - standByHatchmon_Count;
            for (int i = 0; i < standByHatchmon_Count; i++)
            {
                CreateStandByHatchmonSlot();
            }
        }

        foreach (var hatchmon in allStandByHitchmonData)
        {
            var hatchmonData = hatchmon.hatchmonStatsData; // <- in-game data
            var hatchmonProperty = HatchmonDataManager.Instance.GetHatchmonPropertyByHatchId(hatchmonData.id);

            var slot = FindEmptyStandbySlot(); // <- assign to this emtpy slot

            var hatchmonUIObj = Instantiate(hitchmon_DragableSlot_Prefab);
            hatchmonUIObj.name = hatchmonData.id;

            // assign the data on the ui
            hatchmonUIObj.AddComponent<HatchmonInfoUI>()
                .SetData(
                    hatchmonData.id,
                    hatchmonProperty.grade,
                    hatchmonProperty.creatureType,
                    hatchmonData.level,
                    0, // <- means the star level will be hide
                    ""); 

            hatchmonUIObj.transform.SetParent(slot);
            hatchmonUIObj.transform.localPosition = Vector3.zero;
            hatchmonUIObj.transform.localScale = Vector3.one;

            // can be drag
            hatchmonUIObj.AddComponent<DragableItemUI>();

            // can be drop
            //slot.GetComponent<DropableItemUI>().enabled = true;

            // can be click
            slot.GetComponent<ClickableItem>().enabled = true;
        }
    }

    //private void UpdateDataToDatabase()
    //{
    //    // update the battlefield runtime data and get ready to push the data back to the database when then whole panel exist
    //    var runtimeData = MiningAreaDataManger.Instance.GetRuntimeData();

    //    for (int i = 0; i < allMiningHatchingDeckSlots.Length; i++)
    //    {
    //        // slot can be emtpy, but remember also clear the data, if the hatchmon was drag out from the deck
    //        if (allMiningHatchingDeckSlots[i].childCount <= 0)
    //        {
    //            runtimeData.hatchmonsData[i].id = string.Empty;
    //            runtimeData.hatchmonsData[i].locationIndex = 0;
    //            continue;
    //        }

    //        // slot is not empty
    //        var hatchmonObj = allMiningHatchingDeckSlots[i].GetChild(0);

    //        // update the runtime data
    //        runtimeData.hatchmonsData[i].id = hatchmonObj.name;
    //        runtimeData.hatchmonsData[i].locationIndex = i + 1;
    //        //runtimeData.hatchmonsData[i].starLevel = hatchmonObj.GetComponent<DragableHatchmonUI>().GetStarLevel();

    //        // upload the hatchmon database to who is really on battlefield area
    //        // whoeveer is on the battle camp change the occupyArea to BattleFieldArea
    //        HatchmonDataManager.Instance.GetSinglehatchmonRuntimeStateDataWithId(hatchmonObj.name)
    //        .occupyArea = EAreaType.MININGAREA;
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
    //    MiningAreaDataManger.Instance.SendRunTimeDataToDatabase();
    //}

    public Transform FindEmptyStandbySlot()
    {
        foreach (Transform slot in allstandByHitchmonSlots)
        {
            if (slot.childCount <= 0)
            {
                slot.gameObject.SetActive(true);
                return slot;
            }
        }
        CreateStandByHatchmonSlot(); // no enough slot, just create one more
        return FindEmptyStandbySlot();
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
