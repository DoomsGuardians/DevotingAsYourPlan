using UnityEngine;
using Cysharp.Threading.Tasks;

public abstract class EventEffectSO : ScriptableObject, IEventEffect
{
    // 默认异步版本：推荐新效果全部用这个写
    public virtual async UniTask ApplyAsync()
    {
        // 调用同步旧版本，保持兼容
        Apply();
        await UniTask.Yield(); // 保证格式为 async，避免死锁
    }

    // 同步版本（老方法），如果子类需要可以重写
    public abstract void Apply();

    // 子类描述信息（可以根据需要重写）
    public abstract string Description { get; }
}
