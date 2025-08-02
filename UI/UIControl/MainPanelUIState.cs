using UnityEngine;

public class MainPanelUIState : IUIPanelState
{
    private readonly GameObject panelObj;

    public UIPanelType panelType { get => UIPanelType.MAINPANEL; }

    public MainPanelUIState(GameObject panelObj)
    {
        this.panelObj = panelObj;
    }

    public void OnEnter()
    {
        if(!panelObj.activeSelf)
            panelObj.SetActive(true);
    }

    public void Tick()
    {

    }

    public void OnExit()
    {
        // no need to disactive
        //panelObj.SetActive(false);
    }
}
