using UnityEngine;

[CreateAssetMenu(fileName = "事件结束效果", menuName = "Events/Effects/事件结束效果")]
public class ShowEndEffect : EventEffectSO
{
    [Tooltip("事件结束的新名字")]
    public string endName = "事件结束";
    [TextArea(10,60)]
    [Tooltip("事件结束的新描述")]
    public string desc = "事件结束描述";
    public Sprite img;

    // 这里是触发效果的具体实现
    public override void Apply(EventInstance eventInstance)
    {
    }

    public override async Cysharp.Threading.Tasks.UniTask ApplyAsync(EventInstance eventInstance)
    {
        // 调用 EventInstance 的 ShowEnd 方法
        await eventInstance.ShowEnd(endName, desc, img);  // 你可以根据需要传递合适的参数
    }
    
    public override string Description => "显示事件结束信息";
}