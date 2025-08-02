using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditCampPanel : MonoBehaviour
{
    [SerializeField] private BattleFieldAreaPanel m_BattleFieldAreaPanel;
    
    [SerializeField] private GameObject battleFieldInfoPanel;

    [SerializeField] private GameObject battleCamp_slot_Prefab;
    [SerializeField] private GameObject standByHitchmon_slot_Prefab;
    [SerializeField] private GameObject hitchmon_DragableSlot_Prefab;

    [SerializeField] private SSkinUIData battleCamp_SlotSkinData;
    [SerializeField] private SSkinUIData standByHitchmon_SlotSkinData;

    [SerializeField] private Transform battleCamp_Grid_Content_Group;
    [SerializeField] private Transform standByHatchmon_Grid_Content_Group;

    [SerializeField] private Button back_button;
    [SerializeField] private Button applyCampEdit_Button;
    [SerializeField] private Button resetStage_Button;

    [SerializeField] private Sprite buttonDeactiveSprite;
    [SerializeField] private Sprite buttonActiveSprite;

    [SerializeField] private ScrollRect m_ScrollRect;

    private List<Transform> allstandByHitchmonSlots = new();

    private int standByHatchmon_MaxCount = 5; // <- use to pre create slots (at least how many should show on the screen even emtpy) on the grid and see if need extra slot when more data add in

    private Transform[] allBattleCampSlots;
    private readonly int battleCampMaxSlotCount = 5; // <- fixed 5 slot allow in the battle camp

    // cache data for getting back the previous setup battle camp hatchmons if the user doesn't want to apply the data
    [SerializeField] private Transform[] preiousHatchmonsOnBattleCamp = new Transform[5];
    [SerializeField] private int[] preiousHatchmonsOnBattleCampStarLevel = new int[5];

    // to record all the hatchmoms should assign to the standby slots
    private List<HatchmonData> allStandByHitchmonData = new();

    /// <summary>
    /// just to record the changed battle camp hatchmon hatchId to update the database
    /// </summary>
    private string[] campChangeLog = new string[5];

    private ConfrimPoppu resetStageWindow;

    private bool isDropEventRegistered = false;

    /// <summary>
    /// when panel open and things need to be reset
    /// </summary>
    private void OnEnable()
    {
        m_ScrollRect.verticalNormalizedPosition = 1f;

        isDropEventRegistered = false;

        ResgisterDropEvent();
    }

    private void OnDisable()
    {
        applyCampEdit_Button.interactable = false;
        applyCampEdit_Button.GetComponent<Image>().sprite = buttonDeactiveSprite;

        UnResgisterDropEvent();
    }

    private void Awake()
    {
        PreInitBattleCampSlots();

        PreInitStandbyHatchmonSlots();

        back_button.onClick.AddListener(() => {
            ToSwapBattleCampHatchmonSlotsNoApplyClick();
            gameObject.SetActive(false);
            battleFieldInfoPanel.SetActive(true);
            m_BattleFieldAreaPanel.StartRewardTimer();
        });

        applyCampEdit_Button.onClick.AddListener(() =>
        {
            OnApplyEditButtonClick();
        });

        resetStage_Button.onClick.AddListener(() =>
        {
            var window = UIManager.Instance.GetPopupWindowByName(EPoppuWindowType.Area_BattleField_ResetStage);
            window.SetActive(true);

            resetStageWindow = window.GetComponent<ConfrimPoppu>();

            resetStageWindow.OnConfirmButtonClicked += OnConfirmToResetStage;
            resetStageWindow.OnGoBackButtonClicked += OnResetStageBackButtonClick; // add event handler for the go back button
        });

        // confirm the grid fit to all screen size
        standByHatchmon_Grid_Content_Group.gameObject.AddComponent<GridLayoutGroupCellsScreenResizing>().ResizeCells();
    }

    private void PreInitBattleCampSlots()
    {
        allBattleCampSlots = new Transform[battleCampMaxSlotCount];

        // pre generate 10 slots for ready-use
        for (int i = 0; i < allBattleCampSlots.Length; i++)
        {
            GameObject slot = Instantiate(battleCamp_slot_Prefab, battleCamp_Grid_Content_Group);
            slot.GetComponent<RectTransform>().localPosition = Vector3.zero;

            // is will add the needed image component directly
            SkinUISlot skin = slot.AddComponent<SkinUISlot>();
            skin.enabled = true;
            skin.skinData = battleCamp_SlotSkinData;

            //// this slot has star level
            //slot.AddComponent<BattleCampSlotUI>();

            // can be drop
            var dropableHatchmonBattleCampUI = slot.AddComponent<DropableHatchmonBattleCampUI>();
            dropableHatchmonBattleCampUI.SlotType = ESlotUIType.DECK;

            // can be click
            slot.AddComponent<ClickableItem>();

            //allBattleCampSlots.Add(slot.AddComponent<MercenarySummonSlot>());
            allBattleCampSlots[i] = slot.transform;
        }
    }

    /// <summary>
    /// grab all the BattleFieldAreaPanel hatchmons ui object (which they were accurate instance from the database)
    /// so can just swap them into this panels' battle camp slots
    /// </summary>
    public void FromSwapBattleCampHatchmonSlots()
    {
        foreach (var slot in allBattleCampSlots)
        {
            foreach (Transform item in slot)
            {
                Destroy(item.gameObject);
            }
        }

        // whatever hatchmons was assigned on the battleFieldPanel, just move the hatchmon objects to this panel
        var allFromBattleCampSlots = m_BattleFieldAreaPanel.GetAllBattleCampSlots();
        for (int i = 0; i < allBattleCampSlots.Length; i++)
        {
            var hatchmonObj = allFromBattleCampSlots[i].GetChild(0);
            hatchmonObj.SetParent(allBattleCampSlots[i]);
            hatchmonObj.localPosition = Vector3.zero;

            preiousHatchmonsOnBattleCamp[i] = hatchmonObj;

            var ui = hatchmonObj.GetComponent<HatchmonInfoUI>();
            preiousHatchmonsOnBattleCampStarLevel[i] = ui.GetStarLevel();

            // can be drag
            hatchmonObj.GetComponent<DragableItemUI>().enabled = true;
        }
    }

    /// <summary>
    /// grab all the changed hatchmons ui object (which user drag and drop edit the battle camp on this panel)
    /// so just swap them back to the BattleFieldAreaPanel
    /// </summary>
    public void ToSwapBattleCampHatchmonSlotsNoApplyClick()
    {
        // check every hatchmons obj on the current slot
        // store them into an temp array
        // swap them back!
        var allFromBattleCampSlots = m_BattleFieldAreaPanel.GetAllBattleCampSlots();

        for (int i = 0; i < allBattleCampSlots.Length; i++)
        {
            var hatchmonObj = preiousHatchmonsOnBattleCamp[i];
            hatchmonObj.SetParent(allFromBattleCampSlots[i]);
            hatchmonObj.localPosition = Vector3.zero;

            // assign the star level back
            var ui = hatchmonObj.GetComponent<HatchmonInfoUI>();
            ui.UpdateStarUI(preiousHatchmonsOnBattleCampStarLevel[i]);

            // can not drag
            hatchmonObj.GetComponent<DragableItemUI>().enabled = false;
        }
    }

    /// <summary>
    /// grab all the changed hatchmons ui object (which user drag and drop edit the battle camp on this panel)
    /// so just swap them back to the BattleFieldAreaPanel
    /// </summary>
    public void ToSwapBattleCampHatchmonSlotsWithApplyClicked()
    {
        // check every hatchmons obj on the current slot
        // store them into an temp array
        // swap them back!
        var allbattleCampSlots = m_BattleFieldAreaPanel.GetAllBattleCampSlots();

        for (int i = 0; i < allBattleCampSlots.Length; i++)
        {
            var hatchmonObj = allBattleCampSlots[i].GetChild(0);
            hatchmonObj.SetParent(allbattleCampSlots[i]);
            hatchmonObj.localPosition = Vector3.zero;

            // can not drag
            hatchmonObj.GetComponent<DragableItemUI>().enabled = false;
        }
    }

    ///// <summary>
    ///// grab all the changed hatchmons ui object (which user drag and drop edit the battle camp on this panel)
    ///// so just swap them back to the BattleFieldAreaPanel
    ///// </summary>
    //public void ToSwapBattleCampHatchmonSlotsWithStageResetClicked()
    //{
    //    // check every hatchmons obj on the current slot
    //    // store them into an temp array
    //    // swap them back!
    //    var allbattleCampSlots = m_BattleFieldAreaPanel.GetAllBattleCampSlots();

    //    for (int i = 0; i < allBattleCampSlots.Length; i++)
    //    {
    //        var hatchmonObj = allBattleCampSlots[i].GetChild(0);
    //        hatchmonObj.GetComponent<DragableHatchmonUI>().UpdateStarUI(1); // reset all the star level
    //        hatchmonObj.SetParent(allbattleCampSlots[i]);
    //        hatchmonObj.localPosition = Vector3.zero;
    //    }
    //}

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
        GameObject slot = Instantiate(standByHitchmon_slot_Prefab, standByHatchmon_Grid_Content_Group);
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
    /// usually use when the panel open from the home page
    /// </summary>
    //public void AssignHatchmonsOnStandByGrid(Action onSuccess)
    //{
        // ***FIXING Cache data
        // 1.get total hatchmons owned by the user from the database
        //HatchmonDataManager.Instance.GetDataFromDatabaseCallBack(
        //    data =>
        //    {
        //        OnAllHatchmonsDatabaseDataRecieved(data);
        //        onSuccess?.Invoke();
        //    });

        //OnAllHatchmonsDatabaseDataRecieved(HatchmonDataManager.Instance.Cache_InGame_HatchmonInventory);
        //onSuccess?.Invoke();
    //}

    public void OpenPanel()
    {
        AssignStandbyHatchmonsOnSlot();

        FromSwapBattleCampHatchmonSlots();

        // close and show panels
        gameObject.SetActive(true);
        battleFieldInfoPanel.SetActive(false);
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
                0,
                hatchmonData.hatchId); // <- means the star level will be hide

            slot.gameObject.SetActive(true);

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

    private void OnApplyEditButtonClick()
    {
        // update the battlefield runtime data and get ready to push the data back to the database when then whole panel exist
        //var runtimeData = BattleFieldAreaDataManager.Instance.GetRuntimeData();
        var campData = BattleFieldAreaDataManager.Instance.Cache_Area_Battlefield.campLast;

        for (int i = 0; i < allBattleCampSlots.Length; i++)
        {
            var hatchmonObj = allBattleCampSlots[i].GetChild(0);
            var hatchmonData = hatchmonObj.GetComponent<HatchmonInfoUI>();
            campChangeLog[i] = hatchmonData.HatchId;

            // update the runtime data
            //campData[i].hatchmonId = hatchmonObj.name;
            //campData[i].hatchId = hatchmonData.hatchId;
            //runtimeData.hatchmonsData[i].locationIndex = i + 1;
            //campData[i].battlefield_starGrade = hatchmonObj.GetComponent<DragableHatchmonUI>().GetStarLevel();

            // upload the hatchmon database to who is really on battlefield area
            // whoeveer is on the battle camp change the occupyArea to BattleFieldArea
            //campData[i].placedArea = EAreaType.AREA_BATTLEFIELD.ToString();
            //HatchmonDataManager.Instance.GetSinglehatchmonRuntimeStateDataWithId(hatchmonObj.name)
            //.occupyArea = EAreaType.BATTLEFIELDAREA;
        }

        // what data needs to be reset for all the standby hatchmons
        //foreach (var slots in allstandByHitchmonSlots)
        //{
        //    if (slots.childCount > 0)
        //    {
        //        // then the rest in the standby change the occupyArea to null
        //        HatchmonDataManager.Instance.GetSinglehatchmonRuntimeStateDataWithId(slots.GetChild(0).name)
        //            .occupyArea = EAreaType.NULL;
        //    }
        //}
        //HatchmonDataManager.Instance.SendRunTimeDataToDatabase();
        //BattleFieldAreaDataManager.Instance.SendRunTimeDataToDatabase();

        // update database
        ServerAPIManager.Instance.CallBATTLEFIELDCampChangeFunction(
            campChangeLog[0],
            campChangeLog[1],
            campChangeLog[2],
            campChangeLog[3],
            campChangeLog[4],
            () => {
                ToSwapBattleCampHatchmonSlotsWithApplyClicked();
                gameObject.SetActive(false);
                battleFieldInfoPanel.SetActive(true);
                m_BattleFieldAreaPanel.StartRewardTimer();
            });
    }

    private void OnConfirmToResetStage()
    {
        ServerAPIManager.Instance.CallBATTLEFIRLDStageResetFunction(() =>
        {
            // Debug.Log("Data Completed THen Call");
            m_BattleFieldAreaPanel.GetCurrentBattleFieldArea().BattleStageReset(); // <- this function will spawn and reset the battle area

            // go back to the battlefieldArea UI Panel to see the updates
            ToSwapBattleCampHatchmonSlotsWithApplyClicked();


            gameObject.SetActive(false);

            battleFieldInfoPanel.SetActive(true);

            // after swap the hatchmon then reset
            m_BattleFieldAreaPanel.StageResetAction();

            // make sure to cancel the event when finish
            resetStageWindow.OnConfirmButtonClicked -= OnConfirmToResetStage;
            resetStageWindow.OnGoBackButtonClicked -= OnResetStageBackButtonClick;
        });

        //// update the battlefield runtime data and get ready to push the data back to the database when then whole panel exist
        //var runtimeData = BattleFieldAreaDataManager.Instance.GetRuntimeData();

        //// set all the battle camp hatchmon star level back to level1
        //for (int i = 0; i < allBattleCampSlots.Length; i++)
        //{
        //    var hatchmonObj = allBattleCampSlots[i].GetChild(0);

        //    // update the runtime data
        //    runtimeData.hatchmonsData[i].id = hatchmonObj.name;
        //    runtimeData.hatchmonsData[i].locationIndex = i + 1;
        //    runtimeData.hatchmonsData[i].starLevel = 1;

        //    // update the display as well
        //    hatchmonObj.GetComponent<DragableHatchmonUI>().UpdateStarUI(1);
        //    // upload the hatchmon database to who is really on battlefield area
        //    // whoeveer is on the battle camp change the occupyArea to BattleFieldArea
        //    HatchmonDataManager.Instance.GetSinglehatchmonRuntimeStateDataWithId(hatchmonObj.name)
        //    .occupyArea = EAreaType.BATTLEFIELDAREA;
        //}

        //// what data needs to be reset for all the standby hatchmons
        //foreach (var slots in allstandByHitchmonSlots)
        //{
        //    if (slots.childCount > 0)
        //    {
        //        // then the rest in the standby change the occupyArea to null
        //        HatchmonDataManager.Instance.GetSinglehatchmonRuntimeStateDataWithId(slots.GetChild(0).name)
        //            .occupyArea = EAreaType.NULL;
        //    }
        //}

        //// Remove all the upgrade book item as well, design upgrade item will be remove as the stage is reset
        //runtimeData.upgradeItems.Clear();
        //m_BattleFieldAreaPanel.HideAllStarLevelUpgradeItem();

        //// reset the monster as well
        //var monsterData = runtimeData.monsterData;
        //monsterData.id = string.Empty;
        //monsterData.level = 0;
        //monsterData.hp = 0;

        //// ***FIXING better make a callback to all the code below for wait all the data all set then return
        //// update the data to database
        //HatchmonDataManager.Instance.SendRunTimeDataToDatabase();
        //BattleFieldAreaDataManager.Instance.SendRunTimeDataToDatabase();

        //m_BattleFieldAreaPanel.GetCurrentBattleFieldArea().BattleStageReset(); // <- this function will spawn and reset the battle area
        
        //// go back to the battlefieldArea UI Panel to see the updates
        //ToSwapBattleCampHatchmonSlotsWithApplyClicked();
        //gameObject.SetActive(false);
        //battleFieldInfoPanel.SetActive(true);

        //// make sure to cancel the event when finish
        //resetStageWindow.OnConfirmButtonClicked -= OnConfirmToResetStage;
        //resetStageWindow.OnGoBackButtonClicked -= OnResetStageBackButtonClick;
    }

    private void OnResetStageBackButtonClick()
    {
        // make sure to cancel the event when finish
        resetStageWindow.OnConfirmButtonClicked -= OnConfirmToResetStage;
        resetStageWindow.OnGoBackButtonClicked -= OnResetStageBackButtonClick;
    }

    private void ResgisterDropEvent()
    {
        if (isDropEventRegistered) return;

        for (int i = 0; i < allBattleCampSlots.Length; i++)
        {
            // can be drop
            if (!allBattleCampSlots[i].TryGetComponent<DropableHatchmonBattleCampUI>(out var onDropCom))
            {
                allBattleCampSlots[i].gameObject.AddComponent<DropableHatchmonBattleCampUI>();
                isDropEventRegistered = false;
            }
            else
            {
                onDropCom.OnDropEvent += OnDropHatmonOnBattleCampSlotEvent;
                isDropEventRegistered = true;
            }
        }
    }

    private void UnResgisterDropEvent()
    {
        if (!isDropEventRegistered) return;

        for (int i = 0; i < allBattleCampSlots.Length; i++)
        {
            if (allBattleCampSlots[i].TryGetComponent<DropableHatchmonBattleCampUI>(out var onDropCom))
                onDropCom.OnDropEvent -= OnDropHatmonOnBattleCampSlotEvent;
        }
        isDropEventRegistered = false;
    }

    /// <summary>
    /// only when changes happen on the battle camp slot then turn on the apply button
    /// </summary>
    private void OnDropHatmonOnBattleCampSlotEvent()
    {
        applyCampEdit_Button.interactable = true; // can apply now
        applyCampEdit_Button.GetComponent<Image>().sprite = buttonActiveSprite;

        UnResgisterDropEvent();
    }
}
