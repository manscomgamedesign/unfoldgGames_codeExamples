public interface IUIPanelState
{
    public void OnEnter();
    public void Tick();
    public void OnExit();

    public UIPanelType panelType { get; }
}
