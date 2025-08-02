using UnityEngine;

public enum InventoryType
{
    Item,
    Egg,
    Hatchmon
}

/// <summary>
/// this including three inventory -> item, egg, hatchmon
/// </summary>
public class InventoryPanelUIState : IUIPanelState
{
    private InventoryPanel m_InventoryPanel;
    private readonly GameObject panelObj;

    private InventoryType inventoryType;

    public UIPanelType panelType { get => UIPanelType.FULLSCREENPANEL; }

    public InventoryPanelUIState(GameObject panelObj, InventoryType inventoryType)
    {
        this.panelObj = panelObj;
        this.inventoryType = inventoryType;
        m_InventoryPanel = this.panelObj.GetComponent<InventoryPanel>();
    }

    public void OnEnter()
    {
        m_InventoryPanel.OpenPanel(inventoryType);
    }

    public void Tick()
    {

    }

    public void OnExit()
    {
        panelObj.SetActive(false);
    }
}
