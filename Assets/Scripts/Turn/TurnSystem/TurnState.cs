using Cysharp.Threading.Tasks;

public abstract class TurnState
{
    protected GameManager gameManager;

    public TurnState(GameManager manager)
    {
        this.gameManager = manager;
    }

    public virtual UniTask EnterAsync() => UniTask.CompletedTask;
    public virtual void Update() { }
    public virtual void Exit() { }
}
