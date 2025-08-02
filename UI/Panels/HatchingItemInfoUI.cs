using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Text;
using System.Collections;

/// <summary>
/// this slot is design to have the follow elements
/// 1.hatching Egg image
/// 2.hatching time counter
/// </summary>
public class HatchingItemInfoUI : MonoBehaviour
{
    [SerializeField] private Image hatchingEgg_Image;
    [SerializeField] private TextMeshProUGUI hatchingTimer_Text;

    private string layId;

    // the timer on the reward window
    private TimeSpan currentTime;
    private StringBuilder timeString = new StringBuilder();
    private Coroutine runningTimerCoroutine;

    public string LayId { get { return layId; } }

    private void Awake()
    {
        GetReference();
    }

    private void GetReference()
    {
        if (hatchingEgg_Image == null) hatchingEgg_Image = transform.Find("HatchingEgg_Image").GetComponent<Image>();
        if (hatchingTimer_Text == null) hatchingTimer_Text = transform.Find("HatchingTimer_Text").GetComponent<TextMeshProUGUI>();
    }

    public TimeSpan GetRunningTime()
    {
        return currentTime;
    }

    public void SetData(string hatchingItemId, string hatchingTimer, string layId)
    {
        hatchingEgg_Image.sprite = SpriteManager.Instance.GetSprite(hatchingItemId);

        AssignRunningTimer(hatchingTimer);

        this.layId = layId;
    }

    public void AssignRunningTimer(string time)
    {
        if (string.IsNullOrEmpty(time))
        {
            hatchingTimer_Text.text = string.Empty;
            return;
        }

        hatchingTimer_Text.text = time;
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

    public IEnumerator UpdateRunningTimer()
    {
        while (true)
        {
            // Check if the current time has reached 12 hours, if yes then stop the counter
            if (TimeSpan.Compare(currentTime, new TimeSpan(12, 0, 0)) >= 0)
            {
                StopRunningTimer();
                currentTime = new TimeSpan(12, 0, 0);
                hatchingEgg_Image.sprite = SpriteManager.Instance.GetSprite("receive_Hatchmon_Box");
                break;
            }

            yield return new WaitForSeconds(1f);
            currentTime = currentTime.Add(TimeSpan.FromSeconds(1));
            timeString.Length = 0;
            timeString.AppendFormat("{0:00}:{1:00}:{2:00}", currentTime.Hours, currentTime.Minutes, currentTime.Seconds);
            hatchingTimer_Text.text = timeString.ToString();
        }
    }
}
