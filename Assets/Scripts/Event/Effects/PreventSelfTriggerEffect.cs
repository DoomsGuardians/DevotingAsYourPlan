using UnityEngine;

[CreateAssetMenu(menuName = "Events/Effects/去除自身再次触发")]
public class PreventSelfTriggerEffect : EventEffectSO
{
    public override void Apply(EventInstance instance)
    {
        GameManager.Instance.EventManager.RemoveEventFromPool(instance);
    }

    public override string Description =>
        $"已经使自身无法触发";
}
