using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using Unity.VisualScripting;

public class RewardingAreaPanel : MonoBehaviour
{
    //[Header("Airdrop")]
    //[SerializeField] private GameObject airDop_SlotPrefab;
    //[SerializeField] private GameObject airDopItem_SlotPrefab; // the clickable item
    //[SerializeField] private SSkinUIData airDop_SlotSkinData;
    //[SerializeField] private Transform airDop_Grid_Content_Group;

    [Header("Hatched Hatchmons")]
    [SerializeField] private GameObject hatchedHatchmons_SlotPrefab;
    [SerializeField] private GameObject hatchedHatchmonsItem_SlotPrefab; // the clickable item
    [SerializeField] private SSkinUIData hatchedHatchmons_SlotSkinData;
    [SerializeField] private Transform hatchedHatchmons_Grid_Content_Group;

    [SerializeField] private ScrollRect m_ScrollRect;

    //private Transform[] allAirDopSlots;
    [SerializeField] private Transform[] allHatchedHatchmonsSlots;

    private readonly int maxSlotCount = 5; // <- fixed 10 slot allow each windows

    [Header("Reward Windows")]
    [SerializeField] private BattleFieldReceiveRewardsWindow m_BattleFieldReceiveRewardsWindow;
    [SerializeField] private TrainingReceiveRewardsWindow m_TrainingReceiveRewardsWindow;

    //[SerializeField] private Button ReceiveAll_Button;

    private void Awake()
    {
        PreInitHatchedHatchmonsSlots();
        m_BattleFieldReceiveRewardsWindow.PreInitRewardWindowSlots();
        m_TrainingReceiveRewardsWindow.PreInitRewardWindowSlots();
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

    //private void PreInitAirDopSlots()
    //{
    //    allAirDopSlots = new Transform[maxSlotCount];

    //    // pre generate 10 slots for ready-use
    //    for (int i = 0; i < allAirDopSlots.Length; i++)
    //    {
    //        GameObject slot = Instantiate(airDop_SlotPrefab, airDop_Grid_Content_Group);
    //        slot.GetComponent<RectTransform>().localPosition = Vector3.zero;

    //        // is will add the needed image component directly
    //        SkinUISlot skin = slot.AddComponent<SkinUISlot>();
    //        skin.enabled = true;
    //        skin.skinData = airDop_SlotSkinData;

    //        //// this slot has star level
    //        //slot.AddComponent<BattleCampSlotUI>();

    //        // can be click
    //        slot.AddComponent<ClickableItem>().enabled = false;

    //        allAirDopSlots[i] = slot.transform;

    //        // pre init the UI obj improve performance
    //        // when update just set it active
    //        var uiObj = Instantiate(airDopItem_SlotPrefab);

    //        uiObj.transform.SetParent(slot.transform);
    //        uiObj.transform.localPosition = Vector3.zero;
    //        uiObj.transform.localScale = Vector3.one;

    //        uiObj.SetActive(false);
    //    }
    //    airDop_Grid_Content_Group.gameObject.AddComponent<GridLayoutGroupCellsScreenResizing>();
    //}

    private void PreInitHatchedHatchmonsSlots()
    {
        allHatchedHatchmonsSlots = new Transform[maxSlotCount];

        // pre generate 10 slots for ready-use
        for (int i = 0; i < allHatchedHatchmonsSlots.Length; i++)
        {
            GameObject slot = Instantiate(hatchedHatchmons_SlotPrefab, hatchedHatchmons_Grid_Content_Group);
            slot.GetComponent<RectTransform>().localPosition = Vector3.zero;

            // is will add the needed image component directly
            SkinUISlot skin = slot.AddComponent<SkinUISlot>();
            skin.enabled = true;
            skin.skinData = hatchedHatchmons_SlotSkinData;

            //// this slot has star level
            //slot.AddComponent<BattleCampSlotUI>();

            // can be click
            slot.AddComponent<ClickableItem>().enabled = false;

            allHatchedHatchmonsSlots[i] = slot.transform;

            // pre init the UI obj improve performance
            // when update just set it active
            var uiObj = Instantiate(hatchedHatchmonsItem_SlotPrefab);

            uiObj.transform.SetParent(slot.transform);
            uiObj.transform.localPosition = Vector3.zero;
            uiObj.transform.localScale = Vector3.one;

            uiObj.AddComponent<HatchingItemInfoUI>();

            uiObj.SetActive(false);
        }
    }

    private void AssignHatchedHatchmons()
    {
        //Data
        HatchingEggData[] deckData = HatchingAreaDataManager.Instance.Cache_Area_Hatching.deck;

        for (int i = 0; i < deckData.Length; i++)
        {
            // find the belong slot index on the panel and assign the data
            var slot = allHatchedHatchmonsSlots[i];
            var uiObj = slot.GetChild(0);

            if (string.IsNullOrEmpty(deckData[i].eggId)) continue;

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

            slot.GetComponent<ClickableItem>().enabled = true;
        }
    }

    private void ExecuteAllHatchingTimer()
    {
        foreach (var slot in allHatchedHatchmonsSlots)
        {
            var hatchingItem = slot.GetChild(0);
            if (hatchingItem.gameObject.activeSelf)
                hatchingItem.GetComponent<HatchingItemInfoUI>().StartRunningTimer();
        }
    }

    //private void ReceivedAllEgg()
    //{
    //    foreach (var slot in allHatchedHatchmonsSlots)
    //    {
    //        var hatchingItem = slot.GetChild(0);
    //        if (hatchingItem.gameObject.activeSelf)
    //        {
    //            var hatchingItemInfoUI = hatchingItem.GetComponent<HatchingItemInfoUI>();
    //            if (TimeSpan.Compare(hatchingItemInfoUI.GetRunningTime(), new TimeSpan(12, 0, 0)) >= 0)
    //            {
    //                // Received the new hatchmon!
    //                // call the server api to receive the hatchmon
    //                ServerAPIManager.Instance.CallHatchingReceiveHatchmonFunction(
    //                    hatchingItemInfoUI.transform.GetSiblingIndex(),
    //                    OnSuccess: data =>
    //                    {
    //                        // reset the slot to be droppable, just by disable it then it can be drop again
    //                        hatchingItemInfoUI.gameObject.SetActive(false);

    //                        var window = UIManager.Instance.GetPopupWindowByName(EPoppuWindowType.Area_Hatching_ReceiveHatchmon);
    //                        window.SetActive(true);

    //                        // ***FIXING property data, should use the server static game data later
    //                        var hatchmonPropertyData = HatchmonDataManager.Instance.GetHatchmonPropertyByNameId(data.hatchmonId);

    //                        // update the popu window ui ***FIXING (hard code to update it)
    //                        var hathcmonUI = window.transform.Find("Body").GetChild(0).transform;
    //                        Sprite backgroundColorSprite = null;
    //                        switch (hatchmonPropertyData.grade)
    //                        {
    //                            case EHatchmonGradeType.NORMAL:
    //                                backgroundColorSprite = SpriteManager.Instance.GetSprite("normalGrade");
    //                                break;
    //                            case EHatchmonGradeType.RARE:
    //                                backgroundColorSprite = SpriteManager.Instance.GetSprite("rareGrade");
    //                                break;
    //                            case EHatchmonGradeType.EPIC:
    //                                backgroundColorSprite = SpriteManager.Instance.GetSprite("epicGrade");
    //                                break;
    //                        }
    //                        hathcmonUI.GetChild(0).GetComponent<Image>().sprite = backgroundColorSprite;
    //                        hathcmonUI.GetChild(1).GetComponent<Image>().sprite = SpriteManager.Instance.GetSprite(data.hatchmonId);
    //                        hathcmonUI.GetChild(2).GetComponent<TextMeshProUGUI>().text = data.inGame_Level.ToString();
    //                        Sprite creatureTypeSprite = null;
    //                        switch (hatchmonPropertyData.creatureType)
    //                        {
    //                            case CreatureType.DEVIL:
    //                                creatureTypeSprite = SpriteManager.Instance.GetSprite("devil");
    //                                break;
    //                            case CreatureType.NATURE:
    //                                creatureTypeSprite = SpriteManager.Instance.GetSprite("nature");
    //                                break;
    //                            case CreatureType.MACHINE:
    //                                creatureTypeSprite = SpriteManager.Instance.GetSprite("machine");
    //                                break;
    //                        }
    //                        hathcmonUI.GetChild(3).GetComponent<Image>().sprite = creatureTypeSprite;

    //                        hathcmonUI.transform.parent.GetComponent<ClickableItem>().enabled = true;
    //                    });
    //            }
    //        }
    //    }
    //}

    public void OpenPanel()
    {
        AssignHatchedHatchmons();
        ExecuteAllHatchingTimer();

        m_BattleFieldReceiveRewardsWindow.AssignRewardWindowRunningTimer(BattleFieldAreaDataManager.Instance.GetCurrentRunningTime());
        m_BattleFieldReceiveRewardsWindow.StartRunningTimer();
        m_BattleFieldReceiveRewardsWindow.CallAssignRewardItemsAction();

        m_TrainingReceiveRewardsWindow.AssignRewardWindowRunningTimer(TrainingAreaDataManager.Instance.GetCurrentRunningTime());
        m_TrainingReceiveRewardsWindow.StartRunningTimer();
        m_TrainingReceiveRewardsWindow.CallAssignRewardItemsAction();
    }

    private void OnClickEvent(PointerEventData eventData)
    {
        var clickedItem = eventData.pointerClick;

        var hatchingItemInfoUI = clickedItem.GetComponentInChildren<HatchingItemInfoUI>();

        var uiObj = clickedItem.transform.GetChild(0);

        if (hatchingItemInfoUI == null)
        {
            var window = UIManager.Instance.GetPopupWindowByName(EPoppuWindowType.Area_BattleField_StarUpgradeItemInfo);
            window.SetActive(true);

            // assign the image
            window.transform.Find("Body").GetChild(0).GetChild(0).GetComponent<Image>().sprite = SpriteManager.Instance.GetSprite(uiObj.name);
        }
        else if (hatchingItemInfoUI != null)
        {
            // reach to 12 hours, Get new Hatchmon!
            if (TimeSpan.Compare(hatchingItemInfoUI.GetRunningTime(), new TimeSpan(12, 0, 0)) >= 0)
            {
                // Received the new hatchmon!
                // call the server api to receive the hatchmon
                ServerAPIManager.Instance.CallHatchingReceiveHatchmonFunction(
                    clickedItem.transform.GetSiblingIndex(),
                    OnSuccess: data =>
                    {
                        uiObj.gameObject.SetActive(false);

                        var window = UIManager.Instance.GetPopupWindowByName(EPoppuWindowType.Area_Hatching_ReceiveHatchmon);
                        window.SetActive(true);

                        // ***FIXING property data, should use the server static game data later
                        var hatchmonPropertyData = HatchmonDataManager.Instance.GetHatchmonPropertyByHatchmonId(data.hatchmonId);

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

                        clickedItem.GetComponent<ClickableItem>().enabled = false;
                    });
            }
            // not reach to 12 hours, still hatching, Show the egg info
            else
            {
                var window = UIManager.Instance.GetPopupWindowByName(EPoppuWindowType.Area_Hatching_HatchingItemInfo);
                window.SetActive(true);
                
                // update the popu window ui ***FIXING (hard code to update it)
                var hathingItemUI = window.transform.Find("Body").GetChild(0).transform;
                hathingItemUI.GetChild(0).GetComponent<Image>().sprite = SpriteManager.Instance.GetSprite(uiObj.name);
            }
        }
    }
}
