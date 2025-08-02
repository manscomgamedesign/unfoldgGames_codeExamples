using UnityEngine;

public class UserStatsManager : MonoBehaviour
{
    #region singleTon
    private static UserStatsManager instance;

    public static UserStatsManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new();
                Debug.LogWarning("more than one instance");
            }
            return instance;
        }
    }
    #endregion

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this.gameObject);

        if (userStatsView == null) userStatsView = gameObject.GetComponent<UserStatsView>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AddUserGold(10);
        }
    }

    [SerializeField] private UserStatsView userStatsView;

    // ***FIXING need these user values as well later -> int experiencePoint, int energyAmount, int gemAmount, 
    public void InitUserStats(int experiencePoint, int gemAmount, int goldAmount)
    {
        UpdateUserGold(goldAmount);
    }

    public void UpdateUserGold(int goldAmount)
    {
        userStatsView.Controller.UpdateGoldAmount(goldAmount);
    }

    public void AddUserGold(int addedAmount)
    {
        userStatsView.Controller.AddedGoldAmount(addedAmount);
    }
}
