using UnityEngine;

public class RewardingAreaPanelUIState : IUIPanelState
{
    private RewardingAreaPanel m_RewardingAreaPanel;

    private readonly GameObject panelObj;

    public UIPanelType panelType { get => UIPanelType.FULLSCREENPANEL; }

    public RewardingAreaPanelUIState(GameObject panelObj)
    {
        this.panelObj = panelObj;
        m_RewardingAreaPanel = panelObj.GetComponent<RewardingAreaPanel>();
    }

    public void OnEnter()
    {
        panelObj.SetActive(true);
        m_RewardingAreaPanel.OpenPanel();
    }

    public void Tick()
    {

    }

    public void OnExit()
    {
        panelObj.SetActive(false);
    }
}
