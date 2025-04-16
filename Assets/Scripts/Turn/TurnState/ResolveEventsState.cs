using UnityEngine;
public class ResolveEventsState : TurnState
{
    public ResolveEventsState(GameManager manager) : base(manager) { }

    public override void Enter()
    {
        Debug.Log("进入事件结算阶段");
        //gameManager.CardManager.DrawCards(); // 逻辑封装到CardManager中
        gameManager.TransitionToState(TurnPhase.EndTurn);
    }
}