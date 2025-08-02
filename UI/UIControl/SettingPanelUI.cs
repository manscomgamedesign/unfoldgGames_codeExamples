using UnityEngine;

public class SettingPanelUI : IUIPanelState
{
    private readonly GameObject panelObj;

    public UIPanelType panelType { get => UIPanelType.POPUWINDOW; }

    public SettingPanelUI(GameObject panelObj)
    {
        this.panelObj = panelObj;
    }

    public void OnEnter()
    {
        panelObj.SetActive(true);
    }

    public void Tick()
    {

    }

    public void OnExit()
    {
        panelObj.SetActive(false);
    }
}
