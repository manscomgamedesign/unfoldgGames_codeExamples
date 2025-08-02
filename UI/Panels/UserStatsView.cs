using UnityEngine;
using TMPro;

public class UserStatsView : MonoBehaviour
{
    [Header("Property")]
    // MVC Reference
    [SerializeField] private UserStatsModel model;
    private UserStatsController controller;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI goldAmount_Text;

    public UserStatsController Controller { get { return controller; } }

    private void Awake()
    {
        if (model == null) model = new UserStatsModel();
        if (controller == null) controller = new UserStatsController(model, this);
    }

    public void UpdateGoldAmountUI(int amount)
    {
        goldAmount_Text.text = amount.ToString();
    }
}
