using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TrainingAreaPanel : MonoBehaviour
{
    [SerializeField] private TrainingEditDeckPanel m_EditDeckPanel;
    [SerializeField] private TrainingReceiveRewardsWindow m_Rewarding_Window;

    [SerializeField] private GameObject trainingAreaInfoPanel;

    [SerializeField] private GameObject trainingHatchmonDeck_slot_Prefab;
    [SerializeField] private GameObject hatchmonItem_SlotPrefab;

    [SerializeField] private SSkinUIData trainingHatchmonDeck_SlotSkinData;

    [SerializeField] private Transform trainingHatchmonDeck_Grid_Content_Group;

    [SerializeField] private ScrollRect m_ScrollRect;

    [SerializeField] private Button[] pages_Button = new Button[3]; // <- max three pages
    [SerializeField] private Button editDeck_Button;

    private Transform[] allTrainingHatchmonDeckSlots;
    private List<Transform> allTrainingAreaRewardWindowSlots;

    private readonly int maxDeckSlotCount = 5; // <- fixed 5 slot allow in the battle camp

    [SerializeField] private TextMeshProUGUI powerCount_Text;
    //[SerializeField] private TextMeshProUGUI rewardWindow_RuuningTimer_Text;

    public Transform[] GetAllHatchmonDeckSlots() => allTrainingHatchmonDeckSlots;

    void Awake()
    {
        PreInitDeckSlots();
        m_Rewarding_Window.PreInitRewardWindowSlots();

        m_EditDeckPanel.gameObject.SetActive(false);

        editDeck_Button.onClick.AddListener(() =>
        {
            m_EditDeckPanel.OpenPanel();
        });
    }

    private void OnEnable()
    {
        ClickableItem.OnPointerClickEvent += OnClickEvent;

        // select the first by default when open the panel
        pages_Button[0].Select();

        m_ScrollRect.verticalNormalizedPosition = 1f;
    }

    private void OnDisable()
    {
        ClickableItem.OnPointerClickEvent -= OnClickEvent;
    }

    private void PreInitDeckSlots()
    {
        allTrainingHatchmonDeckSlots = new Transform[maxDeckSlotCount];

        // pre generate 10 slots for ready-use
        for (int i = 0; i < allTrainingHatchmonDeckSlots.Length; i++)
        {
            GameObject slot = Instantiate(trainingHatchmonDeck_slot_Prefab, trainingHatchmonDeck_Grid_Content_Group);
            slot.GetComponent<RectTransform>().localPosition = Vector3.zero;

            // is will add the needed image component directly
            SkinUISlot skin = slot.AddComponent<SkinUISlot>();
            skin.enabled = true;
            skin.skinData = trainingHatchmonDeck_SlotSkinData;

            //// this slot has star level
            //slot.AddComponent<BattleCampSlotUI>();

            // can be drop
            //slot.AddComponent<DropableHatchmonUI>();

            // can be click
            slot.AddComponent<ClickableItem>();

            //allBattleCampSlots.Add(slot.AddComponent<MercenarySummonSlot>());
            allTrainingHatchmonDeckSlots[i] = slot.transform;

            // pre init the UI obj improve performance
            // when update just set it active
            var hatchmonUIObj = Instantiate(hatchmonItem_SlotPrefab);

            // assign the data on the ui
            hatchmonUIObj.AddComponent<HatchmonInfoUI>();

            hatchmonUIObj.transform.SetParent(slot.transform);
            hatchmonUIObj.transform.localPosition = Vector3.zero;
            hatchmonUIObj.transform.localScale = Vector3.one;

            // pre add the component to use in the edit deck panel
            // can be drag if the slot is not on the battle camp slot and switch to the standby slot, so add this component first
            hatchmonUIObj.AddComponent<DragableItemUI>().enabled = false;
        }
    }

    /// <summary>
    /// the actual function showing all the hatachmons on the panel
    /// </summary>
    public void OpenPanel() // <- onSuccess is a callback that make sure all the data are complete before open the UI panel
    {
        AssignDeckHatchmons();

        UpdatePowerCountUI();

        gameObject.SetActive(true);

        StartRewardTimer();

        m_Rewarding_Window.CallAssignRewardItemsAction();
    }

    private void AssignDeckHatchmons()
    {
        //Data
        HatchmonData[] hatchmonsData = TrainingAreaDataManager.Instance.Cache_Area_Training.deck;

        for (int i = 0; i < hatchmonsData.Length; i++)
        {
            // find the belong slot index on the panel and assign the data
            var slot = allTrainingHatchmonDeckSlots[i];
            var hatchmonUIObj = slot.GetChild(0);

            if (string.IsNullOrEmpty(hatchmonsData[i].hatchmonId))
            {
                hatchmonUIObj.gameObject.SetActive(false);
                continue; // slot can be empty but need to hide
            }

            var hatchmonData = hatchmonsData[i];

            // get the info data
            var hatchmonProperty = HatchmonDataManager.Instance.GetHatchmonPropertyByHatchmonId(hatchmonData.hatchmonId);

            // get the ui gameObject
            hatchmonUIObj.name = hatchmonData.hatchmonId;

            // assign the data on the ui
            hatchmonUIObj.GetComponent<HatchmonInfoUI>()
               .SetData(
               hatchmonData.hatchmonId,
               hatchmonProperty.grade,
               hatchmonProperty.creatureType,
               hatchmonData.inGame_Level,
               0, // <- no show the star level
               hatchmonData.hatchId); 

            hatchmonUIObj.gameObject.SetActive(true);
        }
    }

    public void UpdatePowerCountUI()
    {
        powerCount_Text.text = "Power" + TrainingAreaDataManager.Instance.Cache_Area_Training.powerLast.ToString();
    }

    private void OnClickEvent(PointerEventData eventData)
    {
        var clickedItem = eventData.pointerClick;

        // means no object on the slot
        if (clickedItem.transform.childCount <= 0) return;
        if(!clickedItem.transform.GetChild(0).gameObject.activeSelf) return;

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
        // should be inventory item then
        else
        {
            var window = UIManager.Instance.GetPopupWindowByName(EPoppuWindowType.Area_BattleField_StarUpgradeItemInfo);
            window.SetActive(true);

            // assign the image
            window.transform.Find("Body").GetChild(0).GetChild(0).GetComponent<Image>().sprite = SpriteManager.Instance.GetSprite(clickedItem.transform.GetChild(0).name);
        }
    }    

    public void StartRewardTimer()
    {
        m_Rewarding_Window.AssignRewardWindowRunningTimer(TrainingAreaDataManager.Instance.GetCurrentRunningTime());

        m_Rewarding_Window.StartRunningTimer();
    }
}
