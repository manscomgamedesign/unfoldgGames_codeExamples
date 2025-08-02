using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HatchingAreaPanel : MonoBehaviour
{
    [SerializeField] private GameObject deck_slot_Prefab;
    [SerializeField] private GameObject standBy_slot_Prefab;
    [SerializeField] private GameObject itemSlot_Prefab; // <- even dragable but only can drag when on the standby setection

    [SerializeField] private SSkinUIData deck_SlotSkinData;
    [SerializeField] private SSkinUIData standBy_SlotSkinData;

    [SerializeField] private Transform deck_Grid_Content_Group;
    [SerializeField] private Transform standBy_GridContent;

    [SerializeField] private ScrollRect m_ScrollRect;

    private Transform[] allDeckSlots;
    private List<Transform> allStandBySlots = new();

    // use to find out which eggs will be showing on the standby selection
    private List<HatchingEggData> allStandyEggList = new();

    private readonly int maxDeckSlotCount = 5; // <- fixed 5 slot allow in the battle camp
    private int standBySlot_MaxCount = 15; // <- use to pre create slots (at least how many should show on the screen even emtpy) on the grid and see if need extra slot when more data add in

    //TIMER?

    void Awake()
    {
        PreInitDeckSlots();
        PreInitStandbyHatchmonSlots();

        standBy_GridContent.gameObject.AddComponent<GridLayoutGroupCellsScreenResizing>().ResizeCells();
    }

    private void OnEnable()
    {
        ClickableItem.OnPointerClickEvent += OnClickEvent;

        m_ScrollRect.verticalNormalizedPosition = 1f;
    }

    private void OnDisable()
    {
        ClickableItem.OnPointerClickEvent -= OnClickEvent;
    }

    private void PreInitDeckSlots()
    {
        allDeckSlots = new Transform[maxDeckSlotCount];

        // pre generate 10 slots for ready-use
        for (int i = 0; i < allDeckSlots.Length; i++)
        {
            GameObject slot = Instantiate(deck_slot_Prefab, deck_Grid_Content_Group);
            slot.GetComponent<RectTransform>().localPosition = Vector3.zero;

            // is will add the needed image component directly
            SkinUISlot skin = slot.AddComponent<SkinUISlot>();
            skin.enabled = true;
            skin.skinData = deck_SlotSkinData;

            //// this slot has star level
            //slot.AddComponent<BattleCampSlotUI>();

            // can be drop
            //slot.AddComponent<DropableHatchmonUI>();

            // can be click
            slot.AddComponent<ClickableItem>();

            // can be drop
            slot.AddComponent<DropableHatchingItemHatchingAreaUI>();

            //allBattleCampSlots.Add(slot.AddComponent<MercenarySummonSlot>());
            allDeckSlots[i] = slot.transform;

            // pre init the UI obj improve performance
            // when update just set it active
            var uiObj = Instantiate(itemSlot_Prefab);

            // assign the data on the ui
            uiObj.AddComponent<HatchingItemInfoUI>();

            uiObj.transform.SetParent(slot.transform);
            uiObj.transform.localPosition = Vector3.zero;
            uiObj.transform.localScale = Vector3.one;

            //uiObj.SetActive(false);

            // add drag component but can not drag, not for swaping slot purpose
            uiObj.AddComponent<DragableItemUI>().enabled = false;
        }
    }

    private void PreInitStandbyHatchmonSlots()
    {
        for (int i = 0; i < standBySlot_MaxCount; i++)
        {
            CreateStandByHatchmonSlot();
        }
    }

    private void CreateStandByHatchmonSlot()
    {
        GameObject slot = Instantiate(standBy_slot_Prefab, standBy_GridContent);
        slot.GetComponent<RectTransform>().localPosition = Vector3.zero;

        // is will add the needed image component directly
        SkinUISlot skin = slot.AddComponent<SkinUISlot>();
        skin.enabled = true;
        skin.skinData = standBy_SlotSkinData;

        // can be drop
        //slot.AddComponent<DropableItemUI>().enabled = false;

        // can be click
        slot.AddComponent<ClickableItem>().enabled = false;

        allStandBySlots.Add(slot.transform);

        var uiObj = Instantiate(itemSlot_Prefab);
        uiObj.transform.SetParent(slot.transform);
        uiObj.transform.localPosition = Vector3.zero;
        uiObj.transform.localScale = Vector3.one;

        // can update display infos
        uiObj.AddComponent<HatchingItemInfoUI>();

        // can be drag when need
        uiObj.AddComponent<DragableItemUI>().enabled = false;

        //slot.SetActive(false);
    }

    /// <summary>
    /// the actual function showing all the eggs on the panel
    /// </summary>
    public void OpenPanel()
    {
        AssignDeck();

        AssignStandby();
        standBy_GridContent.gameObject.GetComponent<GridLayoutGroupCellsScreenResizing>().ResizeCells();

        gameObject.SetActive(true);

        ExecuteAllHatchingTimer();
    }

    private void AssignDeck()
    {
        //Data
        HatchingEggData[] deckData = HatchingAreaDataManager.Instance.Cache_Area_Hatching.deck;

        for (int i = 0; i < deckData.Length; i++)
        {
            // find the belong slot index on the panel and assign the data
            var slot = allDeckSlots[i];
            var uiObj = slot.GetChild(0);

            if (string.IsNullOrEmpty(deckData[i].eggId))
            {
                uiObj.gameObject.SetActive(false);
                continue; // slot can be empty but need to hide
            }

            // slot has data need to show
            var eggData = deckData[i];

            // get the ui gameObject
            uiObj.name = eggData.eggId;

            //assign the data on the ui
            uiObj.GetComponent<HatchingItemInfoUI>()
                .SetData(
                    eggData.eggId,
                    HatchingAreaDataManager.Instance.GetHatchingItemTimeByIndex(i),
                    eggData.layId);

            uiObj.gameObject.SetActive(true);
        }
    }

    private void AssignStandby()
    {
        // pre action
        //allStandyEggList.Clear();
        foreach (var slot in allStandBySlots)
        {
            slot.gameObject.SetActive(false);
        }

        var eggInventoryData = HatchingAreaDataManager.Instance.Cache_Ingame_EggInventory;
        var filteredData = eggInventoryData.Where(data => string.Equals(data.hatchingStartTime, "0")).ToList();

        //foreach (var eggData in eggInventoryData)
        //{
        //    // time not 0 means the egg has been used, don't show it
        //    if (!string.Equals(eggData.hatchingStartTime, "0")) continue;

        //    // if time is 0 means the egg has not been use yet so show it on the standy slot
        //    allStandyEggList.Add(eggData);
        //}

        // assign data
        foreach (var egg in filteredData)
        {
            var slot = GetAvaiableStandbySlot(); // <- assign to this emtpy slot

            var uiObj = slot.GetChild(0);
            uiObj.name = egg.eggId; // <- override the name

            uiObj.GetComponent<HatchingItemInfoUI>()
                .SetData(
                egg.eggId,
                string.Empty,
                egg.layId); // <- means no time text need to on the item

            // can be click
            slot.GetComponent<ClickableItem>().enabled = true;

            // can be drag
            uiObj.GetComponent<DragableItemUI>().enabled = true;

            slot.gameObject.SetActive(true);
        }
    }

    private Transform GetAvaiableStandbySlot()
    {
        for (int i = 0; i < allStandBySlots.Count; i++)
        {
            var slot = allStandBySlots[i];

            if (slot.childCount > 0 && !slot.gameObject.activeSelf)
                return slot;
        }

        // if need more space
        CreateStandByHatchmonSlot();
        return GetAvaiableStandbySlot();
    }

    private void ExecuteAllHatchingTimer()
    {
        foreach (var slot in allDeckSlots)
        {
            var hatchingItem = slot.GetChild(0);
            if (hatchingItem.gameObject.activeSelf)
                hatchingItem.GetComponent<HatchingItemInfoUI>().StartRunningTimer();
        }
    }

    /// <summary>
    /// Received New Hatchmon logic include
    /// </summary>
    private void OnClickEvent(PointerEventData eventData)
    {
        var clickedItem = eventData.pointerClick;

        // means no object on the slot
        if (clickedItem.transform.childCount <= 0) return;
        if (!clickedItem.transform.GetChild(0).gameObject.activeSelf) return;

        var hatchingItem = clickedItem.transform.GetChild(0);

        // check the running time of the hatching item see if reach to 12 hours to show the correct poppu
        if (hatchingItem.TryGetComponent<HatchingItemInfoUI>(out var hatchmonInfoUI))
        {
            // reach to 12 hours, Get new Hatchmon!
            if (TimeSpan.Compare(hatchmonInfoUI.GetRunningTime(), new TimeSpan(12, 0, 0)) >= 0)
            {
                // Received the new hatchmon!
                // call the server api to receive the hatchmon
                ServerAPIManager.Instance.CallHatchingReceiveHatchmonFunction(
                    clickedItem.transform.GetSiblingIndex(),
                    OnSuccess: data =>
                    {
                        // reset the slot to be droppable, just by disable it then it can be drop again
                        hatchingItem.gameObject.SetActive(false);

                        var window = UIManager.Instance.GetPopupWindowByName(EPoppuWindowType.Area_Hatching_ReceiveHatchmon);
                        window.SetActive(true);

                        // ***FIXING property data, should use the server static game data later
                        var hatchmonPropertyData = HatchmonDataManager.Instance.GetHatchmonPropertyByHatchId(data.hatchmonId);

                        // update the popu window ui ***FIXING (hard code to update it)
                        var hathcmonUI = window.transform.Find("Body").GetChild(0).transform;
                        Sprite backgroundColorSprite = null;
                        switch (hatchmonPropertyData.grade)
                        {
                            case EHatchmonGradeType.NORMAL:
                                backgroundColorSprite = SpriteManager.Instance.GetSprite("normalGrade");
                                break;
                            case EHatchmonGradeType.RARE:
                                backgroundColorSprite = SpriteManager.Instance.GetSprite("rareGrade");
                                break;
                            case EHatchmonGradeType.EPIC:
                                backgroundColorSprite = SpriteManager.Instance.GetSprite("epicGrade");
                                break;
                        }
                        hathcmonUI.GetChild(0).GetComponent<Image>().sprite = backgroundColorSprite;
                        hathcmonUI.GetChild(1).GetComponent<Image>().sprite = SpriteManager.Instance.GetSprite(data.hatchmonId);
                        hathcmonUI.GetChild(2).GetComponent<TextMeshProUGUI>().text = data.inGame_Level.ToString();
                        Sprite creatureTypeSprite = null;
                        switch (hatchmonPropertyData.creatureType)
                        {
                            case CreatureType.DEVIL:
                                creatureTypeSprite = SpriteManager.Instance.GetSprite("devil");
                                break;
                            case CreatureType.NATURE:
                                creatureTypeSprite = SpriteManager.Instance.GetSprite("nature");
                                break;
                            case CreatureType.MACHINE:
                                creatureTypeSprite = SpriteManager.Instance.GetSprite("machine");
                                break;
                        }
                        hathcmonUI.GetChild(3).GetComponent<Image>().sprite = creatureTypeSprite;
                    });
            }
            // not reach to 12 hours, still hatching, Show the egg info
            else
            {
                var window = UIManager.Instance.GetPopupWindowByName(EPoppuWindowType.Area_Hatching_HatchingItemInfo);
                window.SetActive(true);

                // update the popu window ui ***FIXING (hard code to update it)
                var hathingItemUI = window.transform.Find("Body").GetChild(0).transform;
                hathingItemUI.GetChild(0).GetComponent<Image>().sprite = SpriteManager.Instance.GetSprite(hatchingItem.name);
            }
        }
    }
}
