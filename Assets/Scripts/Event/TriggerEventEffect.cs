using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Events/Effects/TriggerEvent")]
public class TriggerEventEffect : EventEffectSO
{
    public EventNodeData eventToQueue;

    public override void Apply()
    {
        if (eventToQueue != null)
        {
            EventGraphManager.QueueEvent(eventToQueue);
            Debug.Log($"[事件效果] 已排入事件链队列：{eventToQueue.eventName}");
        }
    }

    public override string Description => $"排入事件：{eventToQueue?.eventName}";
}

