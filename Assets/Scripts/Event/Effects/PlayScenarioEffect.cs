using UnityEngine;
using Naninovel;
using Cysharp.Threading.Tasks;
using Mono.CSharp;

[CreateAssetMenu(menuName = "Events/Effects/播放剧本")]
public class PlayScenarioEffect : EventEffectSO
{
    [Tooltip("剧本名称")]
    public string scriptName;

    [Tooltip("是否等待剧本播放完成再继续流程")]
    public bool waitForFinish = true;

    public override async Cysharp.Threading.Tasks.UniTask ApplyAsync(EventInstance instance)
    {
        if (!Engine.Initialized) await RuntimeInitializer.Initialize();
        // 1.Enable Naninovel input.
        var inputManager = Engine.GetService<IInputManager>();
        inputManager.ProcessInput = true;
        
        if (string.IsNullOrEmpty(scriptName))
        {
            Debug.LogWarning("[NaninovelEffect] 未设置剧本名");
            return;
        }

        var player = Engine.GetService<IScriptPlayer>();
        if (player == null)
        {
            Debug.LogError("[NaninovelEffect] 无法获取 IScriptPlayer");
            return;
        }

        try
        {
            Debug.Log($"[NaninovelEffect] 播放剧本：{scriptName}");

            if (waitForFinish)
            {
                // Play the script
                await player.LoadAndPlay(scriptName);

                // Wait for the script to finish using the event system
                await WaitForScriptToFinish(player);
            }
            else
            {
                _ = player.LoadAndPlay(scriptName); // Fire and forget if not waiting for completion
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[NaninovelEffect] 播放剧本时出错：{ex.Message}");
        }

        return;
    }

        // Helper method to wait for script to finish
    private async Cysharp.Threading.Tasks.UniTask WaitForScriptToFinish(IScriptPlayer player)
    {
        // This callback will be called when the script finishes
        var completionTask = new Cysharp.Threading.Tasks.UniTaskCompletionSource();
        var script = player.PlayedScript;
        player.OnStop += (script) => completionTask.TrySetResult(); // Subscribe to script finished event

        await completionTask.Task; // Wait until the script finishes

        // Unsubscribe from the events once done
        player.OnStop -= (script) => completionTask.TrySetResult();
    }

    public override void Apply(EventInstance instance)
    {
    }

    public override string Description =>
        $"播放 Naninovel 剧本【{scriptName}】" +
        (waitForFinish ? "（等待完成）" : "（异步播放）");
}
