using Animancer;
using Cysharp.Threading.Tasks;
using UnityEngine;

public static class EventInstanceExtensions
{
    public static async UniTask PlayAndDestroyAfterAnim(this EventInstance evt)
    {
        if (evt.animancer != null && evt.clips != null && evt.clips.Count > 0)
        {
            // 确保索引不越界
            var clipIndex = Mathf.Min(3, evt.clips.Count - 1);  // 只使用有效的索引
            var clip = evt.clips[clipIndex];

            if (clip != null)
            {
                // 播放动画
                var state = evt.animancer.Play(clip);
                if (state != null)
                {
                    // 等待动画播放完毕
                    await UniTask.Delay((int)(clip.Clip.length * 1000+500)); 
                }
                else
                {
                    Debug.LogError("[EventInstanceExtensions] 无法播放动画，AnimancerState 为 null");
                }
            }
            else
            {
                Debug.LogError("[EventInstanceExtensions] 无效的动画剪辑");
            }
        }
        else
        {
            Debug.LogError("[EventInstanceExtensions] Animancer 或动画剪辑列表为空");
        }

        // 销毁事件对象
        Object.Destroy(evt.gameObject);
    }
}
