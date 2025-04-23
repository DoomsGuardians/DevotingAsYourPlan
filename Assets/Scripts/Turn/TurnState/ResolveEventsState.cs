using UnityEngine;
using Cysharp.Threading.Tasks;
public class ResolveEventsState : TurnState
{
    public ResolveEventsState(GameManager manager) : base(manager) { }

    public override async UniTask EnterAsync()
    {
        Debug.Log("进入事件结算阶段");
        await gameManager.ResolveEventEffect(); // ✅ 先处理事件
        await UniTask.Yield(); // ✅ 然后给 UI 留出一帧渲染空间
        await gameManager.TransitionToStateAsync(TurnPhase.NPCAction); // ✅ 再切换到下个状态（如果你有）
    }

}