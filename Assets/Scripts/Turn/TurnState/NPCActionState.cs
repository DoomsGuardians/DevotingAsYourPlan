using UnityEngine;
public class NPCActionState : TurnState
{
    public NPCActionState(GameManager manager) : base(manager) { }

    public override void Enter()
    {
        Debug.Log("进入角色行动阶段");
        gameManager.TransitionToState(TurnPhase.ResolveEvents);
    }
}