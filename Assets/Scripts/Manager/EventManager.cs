using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EventManager
{
    public List<EventInstance> activeEvents;

    public void Initialize()
    {
        Debug.Log("事件管理器已初始化");
        activeEvents = new();
    }

    public void ResolveEvents()
    {
        foreach (var evt in activeEvents.ToArray())
        {
            if (evt.resolved) continue;

            // 默认使用配置中的角色来源
            Role role = GameManager.Instance.GetRole(evt.data.sourceRole);
            var context = new EventContext(evt, false, role);
            bool matched = false;

            // 遍历每个处理分支
            foreach (var branch in evt.data.outcomeBranches)
            {
                if (branch.matchConditions.TrueForAll(c => c.Evaluate(context)))
                {
                    Debug.Log($"[事件处理] 事件【{evt.data.eventName}】匹配分支【{branch.label}】");

                    foreach (var effect in branch.effects)
                        effect.Apply();

                    foreach (var transition in branch.transitions)
                        EventGraphManager.QueueEvent(transition.targetEvent);

                    matched = true;
                    break;
                }
            }

            if (!matched && evt.IsExpired())
            {
                Debug.Log($"[事件处理] 事件【{evt.data.eventName}】过期未匹配任何分支");
                // 可以考虑添加 fallback 分支支持
            }

            evt.TickLife();

            if (evt.IsExpired())
                activeEvents.Remove(evt);

            HistoryLog.Log(evt, matched);
        }
    }
    
}

