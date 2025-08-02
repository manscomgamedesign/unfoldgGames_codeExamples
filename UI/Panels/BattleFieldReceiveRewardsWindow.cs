using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleFieldReceiveRewardsWindow : ReceiveRewardsWindowBase<BattleFieldCheckRewardsData>
{
    [SerializeField] private BattleFieldAreaPanel m_BattleFieldAreaPanel;
    [SerializeField] private RectTransform goldItem_MoveToLocation;
    [SerializeField] private RectTransform otherItem_MoveToLocation;

    protected override void Awake()
    {
        base.Awake();

        insurmountableTimeInHour = 24;
    }

    protected override void Start()
    {
        base.Start();

        // the gold is always the first item and always on the content, so just pre init it is fine
        // ui structure is easy so fixed it -> index 0 = item image, 1 = item amount
        var firstItem = allRewardWindowSlots[0].GetChild(0);
        firstItem.GetChild(0).GetComponent<Image>().sprite = SpriteManager.Instance.GetSprite("gold");
        firstItem.GetChild(1).GetComponent<TextMeshProUGUI>().text = "0k";
        allRewardWindowSlots[0].gameObject.SetActive(true);
    }

    protected override Transform CreateRewardWindowSlot()
    {
        GameObject slot = Instantiate(window_SlotPrefab, GridContent_Group);
        slot.GetComponent<RectTransform>().localPosition = Vector3.zero;

        // is will add the needed image component directly
        SkinUISlot skin = slot.AddComponent<SkinUISlot>();
        skin.enabled = true;
        skin.skinData = window_SlotSkinData;

        //// this slot has star level
        //slot.AddComponent<BattleCampSlotUI>();

        // can be drop
        //slot.AddComponent<DropableHatchmonUI>();

        // can be click
        slot.AddComponent<ClickableItem>();

        allRewardWindowSlots.Add(slot.transform);

        // pre init all the item obj as well
        var rewardItemUIObj = Instantiate(item_SlotPrefab);

        rewardItemUIObj.transform.SetParent(slot.transform);
        rewardItemUIObj.transform.localPosition = Vector3.zero;
        rewardItemUIObj.transform.localScale = Vector3.one;

        slot.SetActive(false);

        return slot.transform;
    }

    public override void CallAssignRewardItemsAction()
    {
        base.CallAssignRewardItemsAction();

        ServerAPIManager.Instance.CallBATTLEFIELDCheckRewardsFunction(
          OnSuccess: data =>
          {
              AssignRewardWindowItems(data);
              CallReizeLayoutFunction();
          });
    }

    protected override void AssignRewardWindowItems(BattleFieldCheckRewardsData rewardData)
    {
        base.AssignRewardWindowItems(rewardData);

        // disable all before reassign values
        // start from index 1 because index0 always belong to the gold
        for (int i = 1; i < allRewardWindowSlots.Count; i++)
        {
            allRewardWindowSlots[i].gameObject.SetActive(false);
        }

        var fixedBonusReward = rewardData.fixedBonusReward; // <- all the reward items
        var receivableGold = rewardData.receivableGold; // the gold amount

        var coinSlot = allRewardWindowSlots[0]; // <- this is always the gold, and update the text
        coinSlot.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text = receivableGold.ToString() + "k";
        coinSlot.GetChild(0).name = "gold";
        
        // because item is randonly given, so there might be chance no reward items, so double check how many given items
        if (fixedBonusReward.Length > 0)
        {
            // update to show the reward items on the window
            for (int i = 0; i < fixedBonusReward.Length; i++)
            {
                var sprite = SpriteManager.Instance.GetSprite(fixedBonusReward[i].name);
                var slot = FindRewardWindowEmptySlot();
                var element = slot.GetChild(0);
                element.GetChild(0).GetComponent<Image>().sprite = sprite;
                element.GetChild(1).GetComponent<TextMeshProUGUI>().text = fixedBonusReward[i].amount.ToString();
                element.name = fixedBonusReward[i].name;
                slot.gameObject.SetActive(true);
            }
        }
    }

    public override void OnReceiveRewardsButtonClicked()
    {
        base.OnReceiveRewardsButtonClicked(); 
        // check if running time is over 24 hours

        bool shoudResetStage = (TimeSpan.Compare(currentTime, new TimeSpan(insurmountableTimeInHour, 0, 0)) >= 0);

        // if yes show stage reset pop up and once confirm then reset the stage
        if (shoudResetStage)
        {
            m_BattleFieldAreaPanel.ShowResetStageConfirmWindowPopppu(
                EPoppuWindowType.Area_BattleField_StartNewStage);
        }
        // if not then directly grab the rewards
        else
        {
            // no excess time then directly received items
            ServerAPIManager.Instance.CallBATTLEFIRLDReceiveRewardsFunction(
                OnSuccess: data => // the recieved data is the added inventory
                {
                    // spawn recieced items effect
                    // always gold will be recieved 
                    StartCoroutine(
                        m_RewardItemsUISpawner.SpawnReceivedRewardsSpriteEffect(
                            10,
                            SpriteManager.Instance.GetSprite("gold"),
                            goldItem_MoveToLocation));

                    // only spawn which recieved item, let say if recieved an item called "short sword" then spawn the short swaord items
                    foreach (var item in data.Item2)
                    {
                        StartCoroutine(
                           m_RewardItemsUISpawner.SpawnReceivedRewardsSpriteEffect(
                               10,
                               SpriteManager.Instance.GetSprite(item.name),
                               otherItem_MoveToLocation));
                    }

                    ResetReceivedItems();

                    CallReizeLayoutFunction();
                });
        }
    }

    /// <summary>
    /// recieved all the items anyway
    /// </summary>
    public override void ResetReceivedItems()
    {
        base.ResetReceivedItems();

        // no more rewards showing but no index0 because the gold always show but need to be zero amount
        allRewardWindowSlots[0].GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text = "0k";
        for (int i = 1; i < allRewardWindowSlots.Count; i++)
        {
            allRewardWindowSlots[i].gameObject.SetActive(false);
        }
    }
}
