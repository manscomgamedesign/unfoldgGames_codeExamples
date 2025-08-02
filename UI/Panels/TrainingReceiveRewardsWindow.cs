using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TrainingReceiveRewardsWindow : ReceiveRewardsWindowBase<TrainingCheckRewardsData>
{
    [SerializeField] private RectTransform otherItem_MoveToLocation;

    protected override void Awake()
    {
        base.Awake();

        // check if running time is over 12 hours
        insurmountableTimeInHour = 12;
    }

    protected override void Start()
    {
        base.Start();

        // the gold is always the first item and always on the content, so just pre init it is fine
        // ui structure is easy so fixed it -> index 0 = item image, 1 = item amount
        var firstItem = allRewardWindowSlots[0].GetChild(0);
        firstItem.GetChild(0).GetComponent<Image>().sprite = SpriteManager.Instance.GetSprite("Growth Stone");
        firstItem.GetChild(1).GetComponent<TextMeshProUGUI>().text = "0";
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

    public override void OnReceiveRewardsButtonClicked()
    {
        base.OnReceiveRewardsButtonClicked();

        ServerAPIManager.Instance.CallTRAININGReceiveRewardsFunction(
            OnSuccess: data =>
            {
                // only spawn which recieved item, let say if recieved an item called "short sword" then spawn the short swaord items
                foreach (var item in data)
                {
                    StartCoroutine(
                       m_RewardItemsUISpawner.SpawnReceivedRewardsSpriteEffect(
                           10,
                           SpriteManager.Instance.GetSprite(item.name),
                           otherItem_MoveToLocation));
                }

                // reset the stage time from 0
                ResetWinow();
            });
    }

    public override void ResetReceivedItems()
    {
        base.ResetReceivedItems();

        // no more rewards showing but no index0 because the first item is fixed always show but need to be zero amount
        allRewardWindowSlots[0].GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text = "0";
        for (int i = 1; i < allRewardWindowSlots.Count; i++)
        {
            allRewardWindowSlots[i].gameObject.SetActive(false);
        }
    }

    public override void CallAssignRewardItemsAction()
    {
        base.CallAssignRewardItemsAction();

        ServerAPIManager.Instance.CallTRAININGCheckRewardsFunction(
        OnSuccess: data =>
        {
            AssignRewardWindowItems(data);

            CallReizeLayoutFunction();
        });
    }

    protected override void AssignRewardWindowItems(TrainingCheckRewardsData rewardData)
    {
        base.AssignRewardWindowItems(rewardData);

        // disable all before reassign values
        // start from index 1 because index0 always belong to the gold
        for (int i = 1; i < allRewardWindowSlots.Count; i++)
        {
            allRewardWindowSlots[i].gameObject.SetActive(false);
        }

        var fixedBonusReward = rewardData.fixedBonusReward; // <- all the reward items
        var receivableGold = rewardData.receivableGrowthStone; // the gold amount

        var coinSlot = allRewardWindowSlots[0]; // <- this is always the gold, and update the text
        coinSlot.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text = receivableGold.amount.ToString();
        coinSlot.GetChild(0).name = "GrowthStone";

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
}
