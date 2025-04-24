using Cysharp.Threading.Tasks;
using UnityEngine;

public class StartTurnState : TurnState
{
    public StartTurnState(GameManager manager) : base(manager) { }
    public override async UniTask EnterAsync()
    {
        const int startAge = 1400;
        Debug.Log("进入开始阶段");
        gameManager.turnStateMachine.TurnNum++;
        Debug.Log($"这是第{gameManager.turnStateMachine.TurnNum}回合");
        await TurnStateHandler.Instance.TurnStateAnim($"{startAge+gameManager.turnStateMachine.TurnNum}年", $"我，{20+gameManager.turnStateMachine.TurnNum}岁");
        await gameManager.ProcessPlayerDefaultTrigger();
    }
}
