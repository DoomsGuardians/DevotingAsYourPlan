using UnityEngine;

[CreateAssetMenu(menuName = "Events/Effects/Debug")]
public class DebugEffect : EventEffectSO
{
    [TextArea]
    public string debugString;
    public override void Apply(EventInstance instance)
    {
        Debug.Log($"事件效果Debug触发了,{debugString}");
    }

    public override string Description => $"事件效果Debug";
}