using System;
using UnityEngine;
using UnityEngine.UI;

public class ConfrimPoppu : MonoBehaviour
{
    [SerializeField] private Button confirm_Button;
    [SerializeField] private Button back_Button;

    public event Action OnConfirmButtonClicked;
    public event Action OnGoBackButtonClicked;

    private void Awake()
    {
        if (confirm_Button == null) confirm_Button = transform.Find("Confirm_Button").GetComponent<Button>();
        if (back_Button == null) back_Button = transform.Find("Back_Button").GetComponent<Button>();

        confirm_Button.onClick.AddListener(OnConfirmButtonClick);
        back_Button.onClick.AddListener(OnGoBackButtonClick);
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
