using UnityEngine;

public class TrainingAreaPanelUIState : IUIPanelState
{
    private TrainingAreaPanel m_TrainingAreaPanel;

    private readonly GameObject panelObj;

    public UIPanelType panelType { get => UIPanelType.FULLSCREENPANEL; }

    public TrainingAreaPanelUIState(GameObject panelObj)
    {
        this.panelObj = panelObj;
        m_TrainingAreaPanel = panelObj.GetComponent<TrainingAreaPanel>();
    }

    public void OnEnter()
    {
        m_TrainingAreaPanel.OpenPanel();
    }

    public void Tick()
    {

    }

    public void OnExit()
    {
        //// update the database data whenever exit this panel
        //if (PlayfabManager.Instance.hasLogin)
        //{
        //    m_TrainingAreaPanel.OnDataUpdate();
        //    // beause the battle field panel has object assoicate with uuid, so also to make sure if any id has been change after edit the panel
        //    //IDDataManager.Instance.SendRunTimeDataToDatabase();
        //}
        //m_TrainingAreaPanel.StopRunningTimer();

        panelObj.SetActive(false);
    }
}
