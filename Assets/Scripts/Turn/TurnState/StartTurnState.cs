using UnityEngine;
using Naninovel;
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
        gameManager.TransitionToState(TurnPhase.DrawCard);
    }
    public async void PlayDialogue(string scriptName)
    {
        if (!Engine.Initialized)
        {
            await RuntimeInitializer.Initialize();
        }
        PlayScript();
    }

    async void PlayScript()
    {
        // 获取 Naninovel 的 ScriptPlayer 服务
        var scriptPlayer = Engine.GetService<IScriptPlayer>();
        // 播放整个脚本
        await scriptPlayer.LoadAndPlay(scriptName);
    }


    
}