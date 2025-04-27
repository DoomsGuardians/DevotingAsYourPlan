using UnityEngine;
using Naninovel;
using Cysharp.Threading.Tasks;
using Mono.CSharp;

[CreateAssetMenu(menuName = "Events/Effects/播放剧本")]
public class PlayScenarioEffect : EventEffectSO
{
    [Tooltip("剧本名称")] public string scriptName;

    [Tooltip("是否等待剧本播放完成再继续流程")] public bool waitForFinish = true;

    public override async Cysharp.Threading.Tasks.UniTask ApplyAsync(EventInstance instance)
    {
        await GameManager.Instance.PlayScenarioAsync(scriptName);
    }

    // Helper method to wait for script to finish
    // private async Cysharp.Threading.Tasks.UniTask WaitForScriptToFinish(IScriptPlayer player)
    // {
    //     // This callback will be called when the script finishes
    //     var completionTask = new Cysharp.Threading.Tasks.UniTaskCompletionSource();
    //     var script = player.PlayedScript;
    //     player.OnStop += (script) => completionTask.TrySetResult(); // Subscribe to script finished event
    //
    //     await completionTask.Task; // Wait until the script finishes
    //
    //     // Unsubscribe from the events once done
    //     player.OnStop -= (script) => completionTask.TrySetResult();
    // }

    public override void Apply(EventInstance instance)
    {
    }

    public override string Description =>
        $"播放 Naninovel 剧本【{scriptName}】" +
        (waitForFinish ? "（等待完成）" : "（异步播放）");
}