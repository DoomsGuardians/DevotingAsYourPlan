using UnityEngine;
public class PlayerActionState : TurnState
{
    public PlayerActionState(GameManager manager) : base(manager) { }

    public override void Enter()
    {
        Debug.Log("进入玩家行动阶段");
    }
}