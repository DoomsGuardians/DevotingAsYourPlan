using Cysharp.Threading.Tasks;
using UnityEngine;

public class StartTurnState : TurnState
{
    public StartTurnState(GameManager manager) : base(manager) { }
    public override async UniTask EnterAsync()
    {
        await gameManager.CheckForEnding();
        Debug.Log("进入开始阶段");
        gameManager.turnStateMachine.TurnNum++;
        Debug.Log($"这是第{gameManager.turnStateMachine.TurnNum}回合");
        if (gameManager.turnTransitionText.TurnStrings[0] != "" && gameManager.turnTransitionText.TurnStrings[1] != "")
        {
            AudioManager.Instance.PlaySFX("turn_transition");
            await TurnStateHandler.Instance.TurnStateAnim(gameManager.turnTransitionText.TurnStrings[0], gameManager.turnTransitionText.TurnStrings[1]);
        }
        else
        {
            AudioManager.Instance.PlaySFX("turn_transition");
            await TurnStateHandler.Instance.TurnStateAnim($"{gameManager.turnTransitionText.startYear + GameManager.Instance.turnStateMachine.TurnNum}年", $"我，{gameManager.turnTransitionText.startAge +GameManager.Instance.turnStateMachine.TurnNum}岁");
            
        }
        await gameManager.ProcessPlayerDefaultTrigger();
    }
}
