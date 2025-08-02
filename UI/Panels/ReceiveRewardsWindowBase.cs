using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ReceiveRewardsWindowBase <T>: MonoBehaviour
{
    [SerializeField] protected RewardItemsUISpawner m_RewardItemsUISpawner;

    [SerializeField] protected GameObject window_SlotPrefab;
    [SerializeField] protected GameObject item_SlotPrefab;
    [SerializeField] protected SSkinUIData window_SlotSkinData;
    [SerializeField] protected Transform GridContent_Group;
    [SerializeField] private Button receiveRewards_Button;
    [SerializeField] private TextMeshProUGUI rewardWindow_RuuningTimer_Text;

    protected List<Transform> allRewardWindowSlots;

    private readonly int maxSlotCount = 10; // <- fixed 10 slot allow in the battle camp

    // the timer on the reward window
    protected TimeSpan currentTime;
    private StringBuilder timeString = new StringBuilder();
    private Coroutine runningTimerCoroutine;

    // time can not excess this hour
    protected int insurmountableTimeInHour;

    private ContentSizeFitter contentGridFiter;
    private ContentSizeFitter windowLayoutFiter;

    protected virtual void Awake()
    {
        windowLayoutFiter = gameObject.GetComponent<ContentSizeFitter>();
        contentGridFiter = GridContent_Group.GetComponent<ContentSizeFitter>();

        receiveRewards_Button.onClick.AddListener(() =>
        {
            OnReceiveRewardsButtonClicked();
        });
    }

    protected virtual void Start()
    {

    }

    public void PreInitRewardWindowSlots()
    {
        allRewardWindowSlots = new List<Transform>();

        // pre generate 10 slots for ready-use
        for (int i = 0; i < maxSlotCount; i++) // <- pre load 10 space
        {
            CreateRewardWindowSlot();
        }

        GridContent_Group.gameObject.AddComponent<GridLayoutGroupCellsScreenResizing>();
    }

    protected virtual Transform CreateRewardWindowSlot()
    {
        return null;
    }

    protected Transform FindRewardWindowEmptySlot()
    {
        // start from index 1 because index0 always belong to the gold
        for (int i = 0; i < allRewardWindowSlots.Count; i++)
        {
            if (!allRewardWindowSlots[i].gameObject.activeSelf)
                return allRewardWindowSlots[i];
        }

        return CreateRewardWindowSlot();
    }

    public bool CheckIfTimeExcess()
    {
        return (TimeSpan.Compare(currentTime, new TimeSpan(insurmountableTimeInHour, 0, 0)) >= 0);
    }

    public void AssignRewardWindowRunningTimer(string time)
    {
        rewardWindow_RuuningTimer_Text.text = time;
        currentTime = TimeSpan.Parse(time);
    }

    public void StartRunningTimer()
    {
        if (runningTimerCoroutine != null)
            runningTimerCoroutine = null;

        runningTimerCoroutine = StartCoroutine(UpdateRunningTimer());
    }

    public void StopRunningTimer()
    {
        if (runningTimerCoroutine != null)
        {
            StopCoroutine(runningTimerCoroutine);
            runningTimerCoroutine = null;
        }
    }

    private IEnumerator UpdateRunningTimer()
    {
        while (true)
        {
            // Check if the current time has reached insurmountableTimeInHour, if yes then stop the counter
            if (TimeSpan.Compare(currentTime, new TimeSpan(insurmountableTimeInHour, 0, 0)) >= 0)
            {
                StopRunningTimer();
                break;
            }

            yield return new WaitForSeconds(1f);
            currentTime = currentTime.Add(TimeSpan.FromSeconds(1));
            timeString.Length = 0;
            timeString.AppendFormat("{0:00}:{1:00}:{2:00}", currentTime.Hours, currentTime.Minutes, currentTime.Seconds);
            rewardWindow_RuuningTimer_Text.text = timeString.ToString();
        }
    }

    public virtual void OnReceiveRewardsButtonClicked()
    {

    }

    public virtual void CallAssignRewardItemsAction()
    {
    }

    protected virtual void AssignRewardWindowItems(T rewardData)
    {
    }

    public virtual void ResetReceivedItems()
    {
    }

    public virtual void ResetWinow()
    {
        ResetReceivedItems();

        // reset the time from zero as well
        AssignRewardWindowRunningTimer("00:00:00");
        StopRunningTimer();
        StartRunningTimer();

        CallReizeLayoutFunction();
    }

    protected void CallReizeLayoutFunction()
    {
        StartCoroutine(ResizingContent());
    }

    /// <summary>
    /// for the design of the full poppu ui panel in this game, beacuse the full poppu has scroll react content size fitter to take control of the every all the children gameObjects
    /// so use this approach to force resizing the grid content by enable and disable the content size fitter component
    /// ***FIXING noticed this might not be a recommend way as it is forcing resource-intensive in Unity, as performance issue happen in the future, this approach has to be change 
    /// </summary>
    /// <returns></returns>
    private IEnumerator ResizingContent()
    {
        contentGridFiter.enabled = true;
        yield return new WaitForSeconds(0.002f); // wait until the end of the current frame
        contentGridFiter.enabled = false;
        windowLayoutFiter.enabled = true;
        yield return new WaitForSeconds(0.002f); // wait until the end of the current frame
        windowLayoutFiter.enabled = false;
    }
}
