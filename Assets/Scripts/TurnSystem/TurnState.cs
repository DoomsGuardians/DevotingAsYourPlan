public abstract class TurnState
{
    protected GameManager gameManager;

    public TurnState(GameManager manager)
    {
        this.gameManager = manager;
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void Exit() { }
}