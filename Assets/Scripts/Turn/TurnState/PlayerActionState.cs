using UnityEngine;
using Cysharp.Threading.Tasks;
public class PlayerActionState : TurnState
{
    public PlayerActionState(GameManager manager) : base(manager) { }

    public override async UniTask EnterAsync()
    {
        Debug.Log("进入玩家行动阶段");
        if (gameManager.turnTransitionText.TurnStrings[4] != "" && gameManager.turnTransitionText.TurnStrings[5] != "")
        {
            AudioManager.Instance.PlaySFX("turn_transition");
            await TurnStateHandler.Instance.TurnStateAnim(gameManager.turnTransitionText.TurnStrings[4], gameManager.turnTransitionText.TurnStrings[5]);
        }
        await UniTask.Yield(); // 保留异步格式
    }
}