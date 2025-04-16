using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EventGraphManager
{
    // public static void TryTriggerNext(EventInstance current, bool success, Role sourceRole)
    // {
    //     var context = new EventContext(current, success, sourceRole);
    //     foreach (var t in current.data.transitions)
    //     {
    //         if (t.conditionGroup.EvaluateAll(context))
    //         {
    //             var next = new EventInstance(t.targetEvent);
    //             GameManager.Instance.EventManager.activeEvents.Add(next);
    //             return;
    //         }
    //     }
    // }
    
    private static List<EventNodeData> pendingEvents = new();

    /// <summary>
    /// 加入一个待检测是否生成的事件（由事件分支或 TriggerEffect 调用）
    /// </summary>
    public static void QueueEvent(EventNodeData data)
    {
        if (!pendingEvents.Contains(data))
        {
            pendingEvents.Add(data);
            Debug.Log($"[事件队列] 已加入等待触发事件：{data.eventName}");
        }
    }

    /// <summary>
    /// 在 NPC 阶段调用，判断条件，生成满足条件的事件
    /// </summary>
    public static void ProcessPending()
    {
        foreach (var data in pendingEvents.ToArray())
        {
            Role role = GameManager.Instance.GetRole(data.sourceRole);
            var context = new EventContext(null, false, role);

            bool canTrigger = data.triggerConditions.TrueForAll(c => c.Evaluate(context));

            if (canTrigger)
            {
                var instance = new EventInstance(data);
                GameManager.Instance.EventManager.activeEvents.Add(instance);

                Debug.Log($"[事件生成] 满足条件 → 创建事件：{data.eventName}");
                pendingEvents.Remove(data);
            }
            else
            {
                Debug.Log($"[事件保留] 条件不满足 → 保留事件：{data.eventName}");
            }
        }
    }
}

