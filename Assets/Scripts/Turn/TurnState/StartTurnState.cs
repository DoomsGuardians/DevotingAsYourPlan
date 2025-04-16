using UnityEngine;
public class StartTurnState : TurnState
{
    public StartTurnState(GameManager manager) : base(manager) { }

    public override void Enter()
    {
        Debug.Log("进入开始阶段");
        gameManager.turnStateMachine.TurnNum++; // 逻辑封装到CardManager中
        Debug.Log($"这是第{gameManager.turnStateMachine.TurnNum}回合");
        gameManager.TransitionToState(TurnPhase.DrawCard);
    }
}