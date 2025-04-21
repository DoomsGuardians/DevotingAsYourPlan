using UnityEngine;
public class ResolveEventsState : TurnState
{
    public ResolveEventsState(GameManager manager) : base(manager) { }

    public override void Enter()
    {
        Debug.Log("进入事件结算阶段");
        gameManager.ResolveEventEffect();
    }
}