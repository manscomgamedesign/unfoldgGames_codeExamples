using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// use for any items showing on the inventory
/// design to have an image and amount text
/// which fixed it to be blow
/// GetChild(0) = image
/// GetChild(1) = item amount text
/// </summary>
public class InventoryItemInfoUI : MonoBehaviour
{
    [SerializeField] private Image item_Image;
    [SerializeField] private TextMeshProUGUI itemAmount_Text;

    private void Awake()
    {
        GetReference();
    }

    private void GetReference()
    {
        if (item_Image == null) item_Image = transform.Find("Item_Image").GetComponent<Image>();
        if (itemAmount_Text == null) itemAmount_Text = transform.Find("itemAmount_Text").GetComponent<TextMeshProUGUI>();
    }

    public void SetData(string itemSpriteName, int itemAmount)
    {
        item_Image.sprite = SpriteManager.Instance.GetSprite(itemSpriteName);

        itemAmount_Text.text = itemAmount.ToString();
    }
}
