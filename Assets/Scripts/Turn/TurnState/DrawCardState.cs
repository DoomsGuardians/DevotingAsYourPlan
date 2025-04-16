using UnityEngine;
public class DrawCardState : TurnState
{
    public DrawCardState(GameManager manager) : base(manager) { }

    public override void Enter()
    {
        Debug.Log("进入抽卡阶段");
        gameManager.DrawCards(); // 逻辑封装到CardManager中
        gameManager.TransitionToState(TurnPhase.PlayerAction);
    }
}