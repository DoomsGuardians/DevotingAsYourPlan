using UnityEngine;
using Naninovel;
using Naninovel.Commands;
public class StartTurnState : TurnState
{
    public StartTurnState(GameManager manager) : base(manager) { }

    private bool isFirstTime = true;
    private string scriptName = "TestEvent";
    public override void Enter()
    {
        if (isFirstTime)
        {
            PlayDialogue(scriptName);
            isFirstTime = false;
        }
        Debug.Log("==========================================");
        Debug.Log("进入开始阶段");
        gameManager.turnStateMachine.TurnNum++; // 逻辑封装到CardManager中
        Debug.Log($"这是第{gameManager.turnStateMachine.TurnNum}回合");
        Debug.Log($"现在玩家行动槽中包含{gameManager.eventHolders[0].childCount}项行动");
        gameManager.ProcessPlayerDefaultTrigger();
    }

    public override void Exit()
    {
    }
    public async void PlayDialogue(string scriptName)
    {
        if (!Engine.Initialized)
        {
            Engine.OnInitializationFinished += PlayScript;
            await RuntimeInitializer.Initialize();
        }
        else
        {
            PlayScript();
        }
    }

    async void PlayScript()
    {
        // 获取 Naninovel 的 ScriptPlayer 服务
        var scriptPlayer = Engine.GetService<IScriptPlayer>();
        // 播放整个脚本
        await scriptPlayer.LoadAndPlay(scriptName);
    }

    async void DebugScript()
    {
        Debug.Log("NaniEngine初始化好了");
    }
    
}