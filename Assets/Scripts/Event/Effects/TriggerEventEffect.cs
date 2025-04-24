using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Events/Effects/触发后续事件")]
public class TriggerEventEffect : EventEffectSO
{
    public EventNodeData eventToQueue;

    public override void Apply(EventInstance instance)
    {
        if (eventToQueue != null)
        {
            
            GameManager.Instance.EventManager.QueueEvent(eventToQueue);
            Debug.Log($"[事件效果] 已排入事件链队列：{eventToQueue.eventName}");
        }
    }

    public override string Description => $"排入事件：{eventToQueue?.eventName}";
}

