using UnityEngine;

public class MiningAreaPanelUIState : IUIPanelState
{
    private MiningAreaPanel m_MiningAreaPanel;

    private readonly GameObject panelObj;

    public UIPanelType panelType { get => UIPanelType.FULLSCREENPANEL; }

    public MiningAreaPanelUIState(GameObject panelObj)
    {
        this.panelObj = panelObj;
        m_MiningAreaPanel = panelObj.GetComponent<MiningAreaPanel>();
    }

    public void OnEnter()
    {
        m_MiningAreaPanel.AssignMiningHatchmonDeck(
         onSuccess: () =>
         {
             panelObj.SetActive(true); // Turn on the UI when the data is loaded successfully
         });
    }

    public void Tick()
    {

    }

    public void OnExit()
    {
        // update the database data whenever exit this panel
        if (PlayfabManager.Instance.hasLogin)
        {
            m_MiningAreaPanel.OnDataUpdate();
            // beause the battle field panel has object assoicate with uuid, so also to make sure if any id has been change after edit the panel
            //IDDataManager.Instance.SendRunTimeDataToDatabase();
        }
        panelObj.SetActive(false);
    }
}
