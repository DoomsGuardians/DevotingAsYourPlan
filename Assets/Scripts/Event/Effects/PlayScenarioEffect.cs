using UnityEngine;
using Naninovel;
using Cysharp.Threading.Tasks;  // 确保你只使用 Cysharp 的 UniTask

[CreateAssetMenu(menuName = "Events/Effects/播放剧本")]
public class PlayScenarioEffect : EventEffectSO
{
    [Tooltip("剧本名称")]
    public string scriptName;

    [Tooltip("是否等待剧本播放完成再继续流程")]
    public bool waitForFinish = true;

    public override async Cysharp.Threading.Tasks.UniTask ApplyAsync()
    {
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
                await player.LoadAndPlay(scriptName); // 等待剧本播放完成
            }
            else
            {
                _ = player.LoadAndPlay(scriptName); // Fire and forget 异步启动
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[NaninovelEffect] 播放剧本时出错：{ex.Message}");
        }

        return;  // 确保返回一个已完成的 UniTask
    }

    public override void Apply()
    {
    }

    public override string Description =>
        $"播放 Naninovel 剧本【{scriptName}】" +
        (waitForFinish ? "（等待完成）" : "（异步播放）");
}
