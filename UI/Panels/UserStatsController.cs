public class UserStatsController
{
    //MVC Reference
    private readonly UserStatsModel model;
    private readonly UserStatsView view;

    public UserStatsController(UserStatsModel model, UserStatsView view)
    {
        this.model = model;
        this.view = view;
    }

    public void UpdateGoldAmount(int amount)
    {
        model.User_GoldAmount = amount;
        view.UpdateGoldAmountUI(model.User_GoldAmount);
    }

    public void AddedGoldAmount(int addedAmount)
    {
        model.AddGold(addedAmount);
        view.UpdateGoldAmountUI(model.User_GoldAmount);
    }
}
