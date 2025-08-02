using UnityEngine;

public class StorePanelUI : IUIPanelState
{
    private readonly GameObject panelObj;

    public UIPanelType panelType { get => UIPanelType.FULLSCREENPANEL; }

    public StorePanelUI(GameObject panelObj)
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
