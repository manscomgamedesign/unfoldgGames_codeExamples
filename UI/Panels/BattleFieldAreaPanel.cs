using System;
using System.Collections.Generic;
using System.Text;
using PlayFab.ClientModels;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Reflection.Emit;
using System.Collections;

public class BattleFieldAreaPanel : MonoBehaviour
{
    [SerializeField] private EditCampPanel m_EditCampPanel;

    [SerializeField] private BattleFieldArea m_BattleFieldArea;

    [SerializeField] private BattleFieldReceiveRewardsWindow m_Rewarding_Window;

    [SerializeField] private GameObject battleFieldInfoPanel;

    [SerializeField] private GameObject battleCamp_slot_Prefab;
    [SerializeField] private GameObject hatchmonItem_SlotPrefab;
    [SerializeField] private GameObject hatchmonStarLevelUpgrade_slot_Prefab;
    [SerializeField] private GameObject hatchmonStarLevelUpgrade_DragableSlot_Prefab;
 
    [SerializeField] private SSkinUIData battleCamp_SlotSkinData;
    [SerializeField] private SSkinUIData hatchmonStarLevelUpgrade_SlotSkinData;
    [SerializeField] private SSkinUIData battleFieldRewardWindow_SlotSkinData;

    // grid parent for battle camp grid
    [SerializeField] private Transform battleCamp_Grid_Content_Group;
    [SerializeField] private Transform hatchmonStarLevelUpgrade_GridContent_Group;

    [SerializeField] private ScrollRect m_ScrollRect;

    [SerializeField] private Button editCamp_Button;

    // ***FIXING might change the value type 
    private Transform[] allBattleCampSlots;
    private Transform[] allhatchmonStarLevelUpgradeSlots;
    private List<Transform> allbattleFieldRewardWindowSlots;

    // the upgrade item amount can be infinitly, but only show max of 5 on total 5 slots and hide the others.
    // but when one has been use and if we still have items in this list then use this list to spawn one of the hide book into the empty slot
    [SerializeField] private List<Transform> starLevelUpgradeItems = new List<Transform>();

    private readonly int battleCampMaxSlotCount = 5; // <- fixed 5 slot allow in the battle camp

    private ConfrimPoppu resetStageConfirmWindow;

    public BattleFieldArea GetCurrentBattleFieldArea() => m_BattleFieldArea;
    public Transform[] GetAllBattleCampSlots() => allBattleCampSlots;
    public void SetConfirmWindow(ConfrimPoppu window) => resetStageConfirmWindow = window;

    private void Awake()
    {
        PreInitBattleCampSlots();
        PreInithatchmonStarLevelUpgradeSlots();
        m_Rewarding_Window.PreInitRewardWindowSlots();

        m_EditCampPanel.gameObject.SetActive(false);

        editCamp_Button.onClick.AddListener(() =>
        {
            OnEditCampButtonClicked();
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
            slot.AddComponent<DropableHatchmonBattleCampUI>().m_BattleFieldAreaPanel = this;

            // can be click
            slot.AddComponent<ClickableItem>();

            //allBattleCampSlots.Add(slot.AddComponent<MercenarySummonSlot>());
            allBattleCampSlots[i] = slot.transform;

            // pre init the UI obj improve performance
            // when update just set it active
            var hatchmonUIObj = Instantiate(hatchmonItem_SlotPrefab);

            // assign the data on the ui
            hatchmonUIObj.AddComponent<HatchmonInfoUI>();            

            hatchmonUIObj.transform.SetParent(slot.transform);
            hatchmonUIObj.transform.localPosition = Vector3.zero;
            hatchmonUIObj.transform.localScale = Vector3.one;

            // can be drag if the slot is not on the battle camp slot and switch to the standby slot, so add this component first
            hatchmonUIObj.AddComponent<DragableItemUI>().enabled = false;
        }
    }

    private void PreInithatchmonStarLevelUpgradeSlots()
    {
        allhatchmonStarLevelUpgradeSlots = new Transform[battleCampMaxSlotCount];

        // pre generate 10 slots for ready-use
        for (int i = 0; i < allBattleCampSlots.Length; i++)
        {
            GameObject slot = Instantiate(hatchmonStarLevelUpgrade_slot_Prefab, hatchmonStarLevelUpgrade_GridContent_Group);
            slot.GetComponent<RectTransform>().localPosition = Vector3.zero;

            // is will add the needed image component directly
            SkinUISlot skin = slot.AddComponent<SkinUISlot>();
            skin.enabled = true;
            skin.skinData = hatchmonStarLevelUpgrade_SlotSkinData;

            //// this slot has star level
            //slot.AddComponent<BattleCampSlotUI>();

            // can be drop
            //slot.AddComponent<DropableHatchmonUI>();

            // can be click
            slot.AddComponent<ClickableItem>();

            //allBattleCampSlots.Add(slot.AddComponent<MercenarySummonSlot>());
            allhatchmonStarLevelUpgradeSlots[i] = slot.transform;

            // Pre-Instantiate the Update books as well!
            // because the upgrade item object share all the level upgrade image on the same  object, so can onlt Instantiate once and just change the image display to performance
            var upgradeItemUIObj = Instantiate(hatchmonStarLevelUpgrade_DragableSlot_Prefab);

            upgradeItemUIObj.transform.SetParent(slot.transform);
            upgradeItemUIObj.transform.localPosition = Vector3.zero;
            upgradeItemUIObj.transform.localScale = Vector3.one;

            //upgradeItemUIObj.AddComponent<UUIDIndentifer>();
            upgradeItemUIObj.AddComponent<HatchmonStarLevelUpgradeItemUI>();
            upgradeItemUIObj.AddComponent<DragableItemUI>().enabled = false;

            upgradeItemUIObj.SetActive(false);
        }
    }

    /// <summary>
    /// what to do when open this panel 
    /// show the battle camp hatchmon, star upgrade books, reward panel
    /// </summary>
    public void OpenPanel()
    {
        AssignBattleCampHatchmons();

        AssignStarUpgradeBooks();

        gameObject.SetActive(true);

        StartRewardTimer();

        m_Rewarding_Window.CallAssignRewardItemsAction();
    }

    private void AssignBattleCampHatchmons()
    {
        //Data
        HatchmonData[] hatchmonsData = BattleFieldAreaDataManager.Instance.Cache_Area_Battlefield.campLast;

        for (int i = 0; i < hatchmonsData.Length; i++)
        {
            var hatchmonData = hatchmonsData[i];
            var slot = allBattleCampSlots[i];

            // get the info data
            var hatchmonProperty = HatchmonDataManager.Instance.GetHatchmonPropertyByHatchmonId(hatchmonData.hatchmonId);
            
            // get the ui gameObject
            var hatchmonUIObj = slot.GetChild(0);
            hatchmonUIObj.name = hatchmonData.hatchmonId;

            // assign the data on the ui
            hatchmonUIObj.GetComponent<HatchmonInfoUI>()
               .SetData(
               hatchmonData.hatchmonId,
               hatchmonProperty.grade,
               hatchmonProperty.creatureType,
               hatchmonData.inGame_Level,
               hatchmonData.battlefield_starGrade,
               hatchmonData.hatchId);
        }
    }

    private void AssignStarUpgradeBooks()
    {
        var bookInventory = BattleFieldAreaDataManager.Instance.Cache_Area_Battlefield.bookInventory;

        for (int i = 0; i < 5; i++) // only five slot avaiable for upgrade items
        {
            // find the belong slot index on the panel and assign the data
            //if ((updateItemData.locationIndex - 6) > 11) return;
            var slot = allhatchmonStarLevelUpgradeSlots[i]; // upgrade items' battle field area location start from 6-11, change the number back to fit the array index 0-5

            //var upgradeItemUIObj = Instantiate(hatchmonStarLevelUpgrade_DragableSlot_Prefab);
            var upgradeItemUIObject = slot.GetChild(0); // <- already instantiate before          
            upgradeItemUIObject.name = bookInventory[i].bookId;

            //upgradeItemUIObject.GetComponent<UUIDIndentifer>().uuid = Guid.Parse(updateItemData.uuid);

            // because I use only one prefab to store all the book upgrade image
            // so just simply use the level to display the correct upgrade level image on the prefab
            var levelStr = bookInventory[i].bookId;

            // if no book at that slot then no need to show
            if (string.IsNullOrEmpty(levelStr)) continue;

            // show the specific upgrade book
            var level = int.Parse(levelStr.Substring(levelStr.Length - 1));
            upgradeItemUIObject.GetComponent<HatchmonStarLevelUpgradeItemUI>()
                .UpdateLevelUI(level);

            // can be drag if the slot is not on the battle camp slot and switch to the standby slot, so add this component first
            upgradeItemUIObject.GetComponent<DragableItemUI>().enabled = true;

            upgradeItemUIObject.gameObject.SetActive(true);
        }
    }

    private void OnEditCampButtonClicked()
    {
        // check if running time is over 24 hours
        bool shoudResetStage = m_Rewarding_Window.CheckIfTimeExcess();

        if (!shoudResetStage)
        {
            m_EditCampPanel.OpenPanel();
        }
        // if reach to 24 hours, can not do edit the camp until the user click confirm to stage reset
        else
        {
            ShowResetStageConfirmWindowPopppu(EPoppuWindowType.Area_BattleField_StageLocked);
        }
    }

    public void ShowResetStageConfirmWindowPopppu(EPoppuWindowType windowType)
    {
        SetConfirmWindow(UIManager.Instance.GetPopupWindowByName(windowType).GetComponent<ConfrimPoppu>());

        resetStageConfirmWindow.gameObject.SetActive(true);

        resetStageConfirmWindow.OnConfirmButtonClicked += OnConfirmToResetStage;
        resetStageConfirmWindow.OnGoBackButtonClicked += OnResetStageBackButtonClick;
    }

    private void HideResetStageConfirmWindowPopppu()
    {
        if (resetStageConfirmWindow == null) return;

        // close the window
        resetStageConfirmWindow.gameObject.SetActive(false);

        // make sure to cancel the event when finish
        resetStageConfirmWindow.OnConfirmButtonClicked -= OnConfirmToResetStage;
        resetStageConfirmWindow.OnGoBackButtonClicked -= OnResetStageBackButtonClick;

        resetStageConfirmWindow = null;
    }

    public void OnConfirmToResetStage()
    {
        ServerAPIManager.Instance.CallBATTLEFIRLDStageResetFunction(
            () =>
            {
                StageResetAction();

                // Debug.Log("Data Completed THen Call");
                m_BattleFieldArea.BattleStageReset(); // <- this function will spawn and reset the battle area

                // after swap the hatchmon then reset
                // ***FIXING
                // open the editor when done!
                // I think make a full screen loading server pending is ok
            });
    }

    public void OnResetStageBackButtonClick()
    {
        HideResetStageConfirmWindowPopppu();
    }

    /// <summary>
    /// everything need to be done when stage reset
    /// </summary>
    public void StageResetAction()
    {
        // this function might occur in other panel for example the rewarding area panel, so don't need to update the below logic if 
        if (!gameObject.activeSelf)
        {
            HideResetStageConfirmWindowPopppu();
            return;
        }

        // reset
        m_Rewarding_Window.ResetWinow();

        // all the bamp camp hatchmon needs to start from level 1
        for (int i = 0; i < allBattleCampSlots.Length; i++)
        {
            allBattleCampSlots[i].GetChild(0).GetComponent<HatchmonInfoUI>().UpdateStarUI(1);
        }

        HideResetStageConfirmWindowPopppu();
    }

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
            var starLevelHolder = slot.GetChild(4);
            for (int i = 0; i < starLevelHolder.childCount; i++)
            {
                starLevelHolder.GetChild(i).gameObject.SetActive(false);
            }

            if(starLevel > 0) starLevelHolder.GetChild(starLevel - 1).gameObject.SetActive(true);
        }
        // else can be an upgrade item or anything else
        else
        {
            var window = UIManager.Instance.GetPopupWindowByName(EPoppuWindowType.Area_BattleField_StarUpgradeItemInfo);
            window.SetActive(true);

            // assign the image
            window.transform.Find("Body").GetChild(0).GetChild(0).GetComponent<Image>().sprite = SpriteManager.Instance.GetSprite(clickedItem.transform.GetChild(0).name);
        }
    }

    /// <summary>
    /// use this to hide (pretend destroy all the upgrade item), because design to remove all the upgrade items if stage reset!
    /// </summary>
    public void HideAllStarLevelUpgradeItem()
    {
        foreach (var slot in allhatchmonStarLevelUpgradeSlots)
        {
            foreach (Transform item in slot)
            {
                item.gameObject.SetActive(false);
            }
        }
    }

    public void StartRewardTimer()
    {
        m_Rewarding_Window.AssignRewardWindowRunningTimer(BattleFieldAreaDataManager.Instance.GetCurrentRunningTime());

        m_Rewarding_Window.StartRunningTimer();
    }
}
