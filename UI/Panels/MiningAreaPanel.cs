using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class MiningAreaPanel : MonoBehaviour
{
    [SerializeField] private MiningEditDeckPanel m_EditDeckPanel;

    [SerializeField] private GameObject miningAreaInfoPanel;

    [SerializeField] private GameObject miningHatchmonDeck_slot_Prefab;
    [SerializeField] private GameObject hatchmon_DragableSlot_Prefab;
    [SerializeField] private SSkinUIData miningHatchmonDeck_SlotSkinData;

    [SerializeField] private Transform miningHatchmonDeck_Grid_Content_Group;
    [SerializeField] private ScrollRect m_ScrollRect;

    [SerializeField] private Button editDeck_Button;

    private Transform[] allMiningHatchmonDeckSlots;

    private readonly int maxDeckSlotCount = 5; // <- fixed 5 slot allow in the battle camp

    void Awake()
    {
        PreInitBattleCampSlots();

        m_EditDeckPanel.gameObject.SetActive(false);

        editDeck_Button.onClick.AddListener(() =>
        {
            m_EditDeckPanel.AssignHatchmonsOnStandByGrid(
                onSuccess: () =>
                {
                    m_EditDeckPanel.FromSwapHatchmonDeckSlots();
                    m_EditDeckPanel.gameObject.SetActive(true);
                    miningAreaInfoPanel.SetActive(false);
                });
        });
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

    private void PreInitBattleCampSlots()
    {
        allMiningHatchmonDeckSlots = new Transform[maxDeckSlotCount];

        // pre generate 10 slots for ready-use
        for (int i = 0; i < allMiningHatchmonDeckSlots.Length; i++)
        {
            GameObject slot = Instantiate(miningHatchmonDeck_slot_Prefab, miningHatchmonDeck_Grid_Content_Group);
            slot.GetComponent<RectTransform>().localPosition = Vector3.zero;

            // is will add the needed image component directly
            SkinUISlot skin = slot.AddComponent<SkinUISlot>();
            skin.enabled = true;
            skin.skinData = miningHatchmonDeck_SlotSkinData;

            //// this slot has star level
            //slot.AddComponent<BattleCampSlotUI>();

            // can be drop
            //slot.AddComponent<DropableHatchmonUI>();

            // can be click
            slot.AddComponent<ClickableItem>();

            //allBattleCampSlots.Add(slot.AddComponent<MercenarySummonSlot>());
            allMiningHatchmonDeckSlots[i] = slot.transform;
        }
    }

    /// <summary>
    /// the actual function showing all the hatachmons on the panel
    /// </summary>
    public void AssignMiningHatchmonDeck(Action onSuccess) // <- onSuccess is a callback that make sure all the data are complete before open the UI panel
    {
        // 2.check which hatchmons can be use on the battle field (condition below: 3.already assign to the battle field before, 4.not on every area which the EAreaType == NULL)
        // 3.find the five hatchmons who already assign to the battle camp and assign to the correct grid
        // ***FIXING Cached might use a cache techs here to compare the cache data and the database data to avoid continuely Instantiate and Destroy and UI to optimize the performance 
        // for now I just destroy the object for fast result

        // get battle field database data
        MiningAreaDataManger.Instance.GetDataFromDatabaseCallBack(
            data =>
            {
                //OnMiningAreaDatabaseDataRecieved(data); // <- update the hatchmon ui
                onSuccess?.Invoke(); // Invoke the onSuccess callback if it is not null
            });
    }

    //public void OnMiningAreaDatabaseDataRecieved(MiningAreaData miningAreaData)
    //{
    //    var allMiningHatmonsData = miningAreaData.hatchmonsData;

    //    //***FIXING should double confirm if the hatchmon is really at the battle field area
    //    //var allBattleCampeHatmonsData = allHatchmons.Where(o => o.hatchmonStatsData.occupyArea == EAreaType.BATTLEFIELDAREA);
    //    foreach (var slot in allMiningHatchmonDeckSlots)
    //    {
    //        foreach (Transform item in slot)
    //        {
    //            Destroy(item.gameObject);
    //        }
    //    }

    //    foreach (var miningHatchmonData in allMiningHatmonsData)
    //    {
    //        if (string.IsNullOrEmpty(miningHatchmonData.id)) continue; // slot can be empty

    //        // find the belong slot index on the panel and assign the data
    //        var slot = allMiningHatchmonDeckSlots[miningHatchmonData.locationIndex - 1];

    //        var hatchmonProperty = HatchmonDataManager.Instance.GetHatchmonPropertyByNameId(miningHatchmonData.id);
    //        var hatchmonStateData = HatchmonDataManager.Instance.GetSinglehatchmonRuntimeStateDataWithId(miningHatchmonData.id);

    //        var hatchmonUIObj = Instantiate(hatchmon_DragableSlot_Prefab);
    //        hatchmonUIObj.name = hatchmonStateData.id;

    //        // assign the data on the ui
    //        hatchmonUIObj.AddComponent<HatchmonInfoUI>()
    //           .OnUpdateUI(
    //           miningHatchmonData.id,
    //           hatchmonProperty.grade,
    //           hatchmonProperty.creatureType,
    //           hatchmonStateData.level,
    //           0); // <- no show the star level

    //        hatchmonUIObj.transform.SetParent(slot);
    //        hatchmonUIObj.transform.localPosition = Vector3.zero;
    //        hatchmonUIObj.transform.localScale = Vector3.one;

    //        // pre add the component to use in the edit deck panel
    //        // can be drag if the slot is not on the battle camp slot and switch to the standby slot, so add this component first
    //        hatchmonUIObj.AddComponent<DragableItemUI>().enabled = false;
    //    }
    //}

    /// <summary>
    /// when panel exist, we should update the battle camp data to database
    /// </summary>
    public void OnDataUpdate()
    {
        // update the runtime data and get ready to push the data back to the database when then whole panel exist
        var runtimeData = MiningAreaDataManger.Instance.GetRuntimeData();

        for (int i = 0; i < allMiningHatchmonDeckSlots.Length; i++)
        {
            if (allMiningHatchmonDeckSlots[i].childCount <= 0) continue; // slot can be empty

            var hatchmonObj = allMiningHatchmonDeckSlots[i].GetChild(0);

            // update the runtime battle camp hatchmon data
            runtimeData.hatchmonsData[i].id = hatchmonObj.name;
            runtimeData.hatchmonsData[i].locationIndex = i + 1;
        }

        MiningAreaDataManger.Instance.SendRunTimeDataToDatabase();
    }

    //***FIXING very hard fix code on update the window ui, please fix later
    private void OnClickEvent(PointerEventData eventData)
    {
        var clickedItem = eventData.pointerClick;

        // means no object on the slot
        if (clickedItem.transform.childCount <= 0) return;
        if (!clickedItem.transform.GetChild(0).gameObject.activeSelf) return;

        // if find this component then it is a hatchmon
        if (clickedItem.transform.GetChild(0).GetComponent<HatchmonInfoUI>())
        {
            var window = UIManager.Instance.GetPopupWindowByName(EPoppuWindowType.Area_BattleField_HatchmonInfo);
            window.SetActive(true);

            var hatchmonInfo = clickedItem.transform.GetChild(0).GetComponent<HatchmonInfoUI>();

            hatchmonInfo.GetCloneValue(out Sprite bg, out Sprite hatchmon
                , out Sprite type, out string levelText, out int starLevel);

            var slot = window.transform.Find("Body").GetChild(0);
            slot.GetChild(0).GetComponent<Image>().sprite = bg;
            slot.GetChild(1).GetComponent<Image>().sprite = hatchmon;
            slot.GetChild(2).GetComponent<TextMeshProUGUI>().text = levelText;
            slot.GetChild(3).GetComponent<Image>().sprite = type;
            slot.GetChild(4).gameObject.SetActive(false); // hide star level
        }
        //// else can be an upgrade item or anything else
        //else
        //{
        //    var window = UIManager.Instance.GetPopupWindowByName("BattleFIeldHatchmonStarLevelUpgradeItemPoppu");
        //    window.SetActive(true);

        //    // assign the image
        //    window.transform.Find("Body").GetChild(0).GetChild(0).GetComponent<Image>().sprite = SpriteManager.Instance.GetSprite(clickedItem.transform.GetChild(0).name);
        //}
    }

    public Transform[] GetAllHatchmonDeckSlots()
    {
        return allMiningHatchmonDeckSlots;
    }
}
