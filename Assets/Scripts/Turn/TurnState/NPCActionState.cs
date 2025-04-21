using UnityEngine;
public class NPCActionState : TurnState
{
    public NPCActionState(GameManager manager) : base(manager) { }

    public override void Enter()
    {
        Debug.Log("进入角色行动阶段");
        Debug.Log($"玩家槽位共{GameManager.Instance.eventHolders[0].childCount}个");
        gameManager.ProcessEventTrigger();
    }
}