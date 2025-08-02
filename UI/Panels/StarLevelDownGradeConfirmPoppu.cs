using System;
using UnityEngine;
using UnityEngine.UI;

public class StarLevelDownGradeConfirmPoppu : MonoBehaviour
{
    [SerializeField] private HatchmonInfoUI from_Slot; // <- the slot to be downgrade
    [SerializeField] private HatchmonInfoUI to_Slot; // <- the slot will be replace
    [SerializeField] private Button confirm_Button;
    [SerializeField] private Button back_Button;

    public event Action OnConfirmButtonClicked;
    public event Action OnGoBackButtonClicked;

    private void Awake()
    {
        // <- not dragging, just use this script to update the data
        if (from_Slot == null) from_Slot = transform.Find("Body").Find("From_Slot").gameObject.AddComponent<HatchmonInfoUI>();
        if (to_Slot == null) to_Slot = transform.Find("Body").Find("To_Slot").gameObject.AddComponent<HatchmonInfoUI>();
        if (confirm_Button == null) confirm_Button = transform.Find("Confirm_Button").GetComponent<Button>();
        if (back_Button == null) back_Button = transform.Find("Back_Button").GetComponent<Button>();

        confirm_Button.onClick.AddListener(OnConfirmButtonClick);
        back_Button.onClick.AddListener(OnGoBackButtonClick);
    }

    public void UpdateSlotsUI(HatchmonInfoUI fromSlot, HatchmonInfoUI toSlot)
    {
        from_Slot.CloneFromTargetUIValue(fromSlot);
        to_Slot.CloneFromTargetUIValue(toSlot);
        to_Slot.UpdateStarUI(1); // <- have to show one star level
    }

    private void OnConfirmButtonClick()
    {
        if (OnConfirmButtonClicked != null)
        {
            gameObject.SetActive(false);
            OnConfirmButtonClicked();
        }
    }

    private void OnGoBackButtonClick()
    {
        if (OnGoBackButtonClicked != null)
        {
            gameObject.SetActive(false);
            OnGoBackButtonClicked();
        }
    }
}
