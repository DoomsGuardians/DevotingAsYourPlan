using UnityEngine;
using Cysharp.Threading.Tasks;
public class NPCActionState : TurnState
{
    public NPCActionState(GameManager manager) : base(manager) { }

    public override async UniTask EnterAsync()
    {
        Debug.Log("进入角色行动阶段");
        await TurnStateHandler.Instance.TurnStateAnim("羊群周游", "世界运转");
        await gameManager.ProcessEventTrigger();
        await UniTask.Yield(); // 保留异步格式
    }
}