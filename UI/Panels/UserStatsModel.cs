using UnityEngine;

[System.Serializable]
public class UserStatsModel
{
    [SerializeField] private int user_ExperiencePoint;
    [SerializeField] private int user_EnergeAmount;
    [SerializeField] private int user_GemAmount;
    [SerializeField] private int user_GoldAmount;

    public int User_ExperiencePoint { get { return user_ExperiencePoint; } set { user_ExperiencePoint = value; } }
    public int User_EnergeAmount { get { return user_EnergeAmount; } set { user_EnergeAmount = value; } }
    public int User_GemAmount { get { return user_GemAmount; } set { user_GemAmount = value; } }
    public int User_GoldAmount { get { return user_GoldAmount; } set { user_GoldAmount = value; } }


    public void AddGold(int addedGoldAmount)
    {
        user_GoldAmount += addedGoldAmount;
    }
}
