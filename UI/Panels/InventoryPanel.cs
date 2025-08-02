using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryPanel : MonoBehaviour
{
    [SerializeField] private ItemInventoryPanel m_ItemInventoryPanel;
    [SerializeField] private EggInventoryPanel m_EggInventoryPanel;
    [SerializeField] private HatchmonInventoryPanel m_HatchmonInventoryPanel;

    [SerializeField] private Button itemInventoryPanel_Button;
    [SerializeField] private Button eggInventoryPanel_Button;
    [SerializeField] private Button hatchmonInventoryPanel_Button;

    private void Awake()
    {
        if (m_ItemInventoryPanel == null) m_ItemInventoryPanel = transform.Find("ItemInventory").GetComponent<ItemInventoryPanel>();
        if (m_EggInventoryPanel == null) m_EggInventoryPanel = transform.Find("EggInventory").GetComponent<EggInventoryPanel>();
        if (m_HatchmonInventoryPanel == null) m_HatchmonInventoryPanel = transform.Find("HatchmonInventory").GetComponent<HatchmonInventoryPanel>();

        itemInventoryPanel_Button.onClick.AddListener(() =>
        {
            m_ItemInventoryPanel.OpenPanel();
            m_EggInventoryPanel.ClosePanel();
            m_HatchmonInventoryPanel.ClosePanel();
        });

        eggInventoryPanel_Button.onClick.AddListener(() =>
        {
            m_ItemInventoryPanel.ClosePanel();
            m_EggInventoryPanel.OpenPanel();
            m_HatchmonInventoryPanel.ClosePanel();
        });

        hatchmonInventoryPanel_Button.onClick.AddListener(() =>
        {
            m_ItemInventoryPanel.ClosePanel();
            m_EggInventoryPanel.ClosePanel();
            m_HatchmonInventoryPanel.OpenPanel();
        });
    }

    public void OpenPanel(InventoryType inventoryType)
    {
        gameObject.SetActive(true);

        switch (inventoryType)
        {
            case InventoryType.Item:
                // defult open the item inventory panel
                itemInventoryPanel_Button.onClick.Invoke();
                itemInventoryPanel_Button.Select();
                break;
            case InventoryType.Egg:
                // defult open the item inventory panel
                eggInventoryPanel_Button.onClick.Invoke();
                eggInventoryPanel_Button.Select();
                break;
            case InventoryType.Hatchmon:
                // defult open the item inventory panel
                hatchmonInventoryPanel_Button.onClick.Invoke();
                hatchmonInventoryPanel_Button.Select();
                break;
        }
    }
}
