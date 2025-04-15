using UnityEngine;
public class EndTurnState : TurnState
{
    public EndTurnState(GameManager manager) : base(manager) { }

    public override void Enter()
    {
        Debug.Log("进入结束阶段");
        gameManager.CardManager.TickCards();
        gameManager.CardManager.RefreshCards();
        gameManager.TransitionToState(TurnPhase.StartTurn);
    }
}