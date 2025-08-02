using UnityEngine;

public class BattleFieldAreaPanelUIState : IUIPanelState
{
    private BattleFieldAreaPanel m_BattleFieldAreaPanel;
    private readonly GameObject panelObj;

    public UIPanelType panelType { get => UIPanelType.FULLSCREENPANEL; }

    public BattleFieldAreaPanelUIState(GameObject panelObj)
    {
        this.panelObj = panelObj;
        m_BattleFieldAreaPanel = panelObj.GetComponent<BattleFieldAreaPanel>();
    }

    public void OnEnter()
    {
        m_BattleFieldAreaPanel.OpenPanel(); //(() =>
        //{
        //    panelObj.SetActive(true); // Turn on the UI when the data is loaded successfully
        //    //m_BattleFieldAreaPanel.StartRunningTimer();
        //});
    }

    public void Tick()
    {

    }

    public void OnExit()
    {
        //// update the database data whenever exit this panel
        //if (PlayfabManager.Instance.hasLogin)
        //{
        //    Debug.Log("Need");
        //    m_BattleFieldAreaPanel.OnDataUpdate();
        //    // beause the battle field panel has object assoicate with uuid, so also to make sure if any id has been change after edit the panel
        //    //IDDataManager.Instance.SendRunTimeDataToDatabase(); 
        //}
        //m_BattleFieldAreaPanel.StopRunningTimer();

        panelObj.SetActive(false);
    }
}
