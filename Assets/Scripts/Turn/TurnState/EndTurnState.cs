using UnityEngine;
using Cysharp.Threading.Tasks;
public class EndTurnState : TurnState
{
    public EndTurnState(GameManager manager) : base(manager) { }

    public override async UniTask EnterAsync()
    {
        Debug.Log("进入结束阶段");
        if (gameManager.turnTransitionText.TurnStrings[10] != "" && gameManager.turnTransitionText.TurnStrings[11] != "")
        {
            AudioManager.Instance.PlaySFX("turn_transition");
            await TurnStateHandler.Instance.TurnStateAnim(gameManager.turnTransitionText.TurnStrings[10], gameManager.turnTransitionText.TurnStrings[11]);
        }
        gameManager.SettleAllRoles();
        gameManager.TickCards();
        gameManager.RefreshCards();
        gameManager.TickEvents();
        gameManager.TickCoolDown();

        await UniTask.Yield(); // 可选：给 UI 一帧显示空隙
        await gameManager.TransitionToStateAsync(TurnPhase.StartTurn);
    }
}