using Cysharp.Threading.Tasks;
using UnityEngine;

public class DrawCardState : TurnState
{
    public DrawCardState(GameManager manager) : base(manager) { }

    public override async UniTask EnterAsync()
    {
        Debug.Log("进入抽卡阶段");
        await gameManager.DrawCardsAsync();
        Debug.Log("抽卡完成，进入玩家行动阶段");
        await gameManager.TransitionToStateAsync(TurnPhase.PlayerAction);
    }
}
