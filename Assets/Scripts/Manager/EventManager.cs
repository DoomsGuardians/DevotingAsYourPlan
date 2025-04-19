using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Assertions;

public class EventManager
{
    public List<EventInstance> activeEvents;
    
    private List<EventNodeData> pendingEvents = new();

    private List<EventNodeData> defaultEvents = new();

    private GameObject eventSlot;

    private RectTransform eventHolder;
    
    public void Initialize(List<EventNodeData> defaultList, GameObject prefab, RectTransform holder)
    {
        activeEvents = new();
        defaultEvents = defaultList;
        eventSlot = prefab;
        eventHolder = holder;
        Debug.Log("事件管理器已初始化");
    }

    #region 处理事件触发
    /// <summary>
    /// 加入一个待检测是否生成的事件（由事件分支或 TriggerEffect 调用）
    /// </summary>
    public void QueueEvent(EventNodeData data)
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
    public void ProcessEventTrigger()
    {
        ProcessDefault();
        ProcessPending();
    }
    private void ProcessPending()
    {
        if(GameManager.Instance.eventHolder.childCount>=3)
            return;
        foreach (var data in pendingEvents.ToArray())
        {
            bool canTrigger = data.triggerConditions.EvaluateAll(data);
            if (canTrigger)
            {
                EventInstance instance = GameObject.Instantiate(eventSlot, eventHolder).GetComponent<EventInstance>();
                instance.Initialize(data);
                activeEvents.Add(instance);

                Debug.Log($"[事件生成] 满足条件 → 创建事件：{data.eventName}");
                pendingEvents.Remove(data);
            }
            else
            {
                Debug.Log($"[事件保留] 条件不满足 → 保留事件：{data.eventName}");
            }
            if(GameManager.Instance.eventHolder.childCount>=3) break;
        }
    }
    private void ProcessDefault()
    {
        if(GameManager.Instance.eventHolder.childCount>=3)
            return;
        foreach (var data in defaultEvents.ToArray())
        {
            bool canTrigger = data.triggerConditions.EvaluateAll(data);

            if (canTrigger)
            {
                EventInstance instance = GameObject.Instantiate(eventSlot, eventHolder).GetComponent<EventInstance>();
                instance.Initialize(data);
                activeEvents.Add(instance);

                Debug.Log($"[事件生成] 满足条件 → 创建事件：{data.eventName}");
            }
            else
            {
                Debug.Log($"[事件保留] 条件不满足 → 保留事件：{data.eventName}");
            }
            if(GameManager.Instance.eventHolder.childCount>=3) 
                break;
        }
    }

    #endregion
    
    #region 处理事件效果
    public void ResolveEventsEffect()
    {
        foreach (var evt in activeEvents.ToArray())
        {
            if (evt.resolved) continue;
            
            bool matched = false;

            // 遍历每个处理分支
            foreach (var branch in evt.data.outcomeBranches)
            {
                if (branch.matchConditions.EvaluateAll(evt))
                {
                    Debug.Log($"[事件处理] 事件【{evt.data.eventName}】匹配分支【{branch.label}】");

                    foreach (var effect in branch.effects)
                        effect.Apply();
                        
                    matched = true;
                    activeEvents.Remove(evt);
                    List<Card> cards = new List<Card>(evt.cardHolder.cards);
                    for(int i = cards.Count - 1; i >= 0; i--)
                    {
                        evt.cardHolder.RemoveCard(cards[i]);
                        GameManager.Instance.playerCardHolder.TransferCard(cards[i]);
                        
                    }
                    GameObject.Destroy(evt.transform.gameObject);
                    cards.Clear();
                    break;
                }
            }
            if (!matched && evt.IsExpired())
            {
                Debug.Log($"[事件处理] 事件【{evt.data.eventName}】过期未匹配任何分支");
                foreach (var effect in evt.data.expiredEffects)
                {
                    effect.Apply();
                }
                activeEvents.Remove(evt);
                if (evt.cardHolder.cards.Count >= 0)
                {
                    List<Card> cards = new List<Card>(evt.cardHolder.cards);
                    for(int i = cards.Count - 1; i >= 0; i--)
                    {
                        evt.cardHolder.RemoveCard(cards[i]);
                        GameManager.Instance.playerCardHolder.TransferCard(cards[i]);
                        
                    }
                    GameObject.Destroy(evt.transform.gameObject);
                }
                GameObject.Destroy(evt.transform.gameObject);
                
            }
            
            HistoryLog.Log(evt, matched);
        }
    }
    #endregion


    #region 处理事件寿命

    public void TickEvents()
    {
        foreach (var evt in activeEvents.ToArray())
        {
            evt.TickLife();
        }
    }

    #endregion

}

