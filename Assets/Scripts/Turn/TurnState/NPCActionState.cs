using UnityEngine;
using Cysharp.Threading.Tasks;
public class NPCActionState : TurnState
{
    public NPCActionState(GameManager manager) : base(manager) { }

    public override async UniTask EnterAsync()
    {
        Debug.Log("进入角色行动阶段");
        await gameManager.ProcessEventTrigger();
        await UniTask.Yield(); // 保留异步格式
    }
}