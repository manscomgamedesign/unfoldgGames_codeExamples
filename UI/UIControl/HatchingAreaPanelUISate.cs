using UnityEngine;

public class HatchingAreaPanelUISate : IUIPanelState
{
    private HatchingAreaPanel m_HathcingAreaPanel;

    private readonly GameObject panelObj;

    public UIPanelType panelType { get => UIPanelType.FULLSCREENPANEL; }

    public HatchingAreaPanelUISate(GameObject panelObj)
    {
        this.panelObj = panelObj;
        m_HathcingAreaPanel = panelObj.GetComponent<HatchingAreaPanel>();
    }

    public void OnEnter()
    {
        m_HathcingAreaPanel.OpenPanel();
    }

    public void Tick()
    {

    }

    public void OnExit()
    {
        //m_HathcingAreaPanel.StopRunningTimer();

        panelObj.SetActive(false);
    }
}
