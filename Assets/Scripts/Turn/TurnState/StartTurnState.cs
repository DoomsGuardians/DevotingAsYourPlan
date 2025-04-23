using Cysharp.Threading.Tasks;
using UnityEngine;

public class StartTurnState : TurnState
{
    private bool isFirstTime = true;
    private string scriptName = "开始脚本";

    public StartTurnState(GameManager manager) : base(manager) { }

    public override async UniTask EnterAsync()
    {
        if (isFirstTime)
        {
            await PlayDialogue(scriptName);
            isFirstTime = false;
        }

        Debug.Log("进入开始阶段");
        gameManager.turnStateMachine.TurnNum++;
        Debug.Log($"这是第{gameManager.turnStateMachine.TurnNum}回合");
        await gameManager.ProcessPlayerDefaultTrigger();
    }

    private async UniTask PlayDialogue(string script)
    {
        Debug.Log($"播放对话：{script}");
        await UniTask.Delay(500); // 假装播放一段对话
    }
}
