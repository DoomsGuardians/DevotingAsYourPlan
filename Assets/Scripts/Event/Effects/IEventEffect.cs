using UnityEngine;
using Cysharp.Threading.Tasks;

public interface IEventEffect
{
    // 支持异步调用
    UniTask ApplyAsync();

    // 同步接口
    void Apply();

    // 描述信息
    string Description { get; }
}
