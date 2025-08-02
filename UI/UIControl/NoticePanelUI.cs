using UnityEngine;

public class NoticePanelUI : IUIPanelState
{
    private readonly GameObject panelObj;

    public UIPanelType panelType { get => UIPanelType.POPUWINDOW; }

    public NoticePanelUI(GameObject panelObj)
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
