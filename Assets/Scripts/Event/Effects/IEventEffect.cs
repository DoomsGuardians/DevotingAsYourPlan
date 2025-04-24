using UnityEngine;
using Cysharp.Threading.Tasks;

public interface IEventEffect
{
    // 支持异步调用
    UniTask ApplyAsync(EventInstance eventInstance);  // 接受 EventInstance 参数

    // 同步接口
    void Apply(EventInstance eventInstance);  // 接受 EventInstance 参数

    // 描述信息
    string Description { get; }
}
