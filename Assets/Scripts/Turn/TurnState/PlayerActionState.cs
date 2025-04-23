using UnityEngine;
using Cysharp.Threading.Tasks;
public class PlayerActionState : TurnState
{
    public PlayerActionState(GameManager manager) : base(manager) { }

    public override async UniTask EnterAsync()
    {
        Debug.Log("进入玩家行动阶段");
        await UniTask.Yield(); // 保留异步格式
    }
}