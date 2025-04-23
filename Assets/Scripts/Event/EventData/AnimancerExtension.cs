using Cysharp.Threading.Tasks;
using UnityEngine;
using Animancer;

public static class AnimancerExtensions
{
    /// <summary>
    /// 等待 AnimancerState 播放完毕（可安全取消）
    /// </summary>
    public static async UniTask WhenStopped(this AnimancerState state, GameObject owner)
    {
        var token = owner.GetCancellationTokenOnDestroy();

        await UniTask.WaitUntil(() => !state.IsPlaying, cancellationToken: token);
    }
}
