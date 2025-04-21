using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine.Assertions;

public class EventManager
{
    
    public List<EventInstance> activeEvents;
    
    private List<EventNodeData> pendingEvents = new();

    private List<EventNodeData> defaultEvents = new();

    private GameObject eventSlot;

    private GameObject actionSlot;

    private List<RectTransform> eventHolders;
    
    private const int MAX_PLAYER_ACTION_COUNT = 4;

    private const int PLAYER_INDEX = 0;
    
    public void Initialize(List<EventNodeData> defaultList, GameObject eventPrefab, GameObject actionPrefab, List<RectTransform> holder)
    {
        activeEvents = new();
        defaultEvents = defaultList;
        eventSlot = eventPrefab;
        actionSlot = actionPrefab;
        eventHolders = holder;
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
        GameManager.Instance.TransitionToState(TurnPhase.EndTurn);
    }
    private void ProcessPending()
    {
        if (GameManager.Instance.eventHolders[PLAYER_INDEX].childCount <= MAX_PLAYER_ACTION_COUNT)
        {
            foreach (var data in pendingEvents.FindAll((EventNodeData s) => (int)s.sourceRole == PLAYER_INDEX)?.ToArray())
            {
                bool canTrigger = data.triggerConditions.EvaluateAll(data);
                if (canTrigger)
                {
                    EventInstance instance = GameObject.Instantiate(actionSlot, eventHolders[PLAYER_INDEX]).GetComponent<EventInstance>();
                    instance.Initialize(data);
                    activeEvents.Add(instance);

                    Debug.Log($"[事件生成] 满足条件 → 创建事件：{data.eventName}");
                    pendingEvents.Remove(data);
                }
                else
                {
                    Debug.Log($"[事件保留] 条件不满足 → 保留事件：{data.eventName}");
                }
                if(GameManager.Instance.eventHolders[PLAYER_INDEX].childCount>=4) break;
            } 
        }

        //处理其他角色
        for (int i = PLAYER_INDEX+1; i <= GameManager.Instance.eventHolders.Count-1 ; i++)
        {
            if (GameManager.Instance.eventHolders[i].childCount >= 3) continue;
            foreach (var data in pendingEvents.FindAll((EventNodeData s) => (int)s.sourceRole == i)?.ToArray())
            {
                bool canTrigger = data.triggerConditions.EvaluateAll(data);
                if (canTrigger)
                {
                    EventInstance instance = GameObject.Instantiate(eventSlot, eventHolders[i]).GetComponent<EventInstance>();
                    instance.Initialize(data);
                    activeEvents.Add(instance);

                    Debug.Log($"[事件生成] 满足条件 → 创建事件：{data.eventName}");
                    pendingEvents.Remove(data);
                }
                else
                {
                    Debug.Log($"[事件保留] 条件不满足 → 保留事件：{data.eventName}");
                }
                if(GameManager.Instance.eventHolders[i].childCount>=3) break;
            }
        }

    }
    private void ProcessDefault()
    {
        //处理其他角色
        for (int i = PLAYER_INDEX+1; i <= GameManager.Instance.eventHolders.Count-1 ; i++)
        {
            Debug.Log($"处理{GameManager.Instance.eventHolders[i].name}");
            if (GameManager.Instance.eventHolders[i].childCount >= 3) continue;
            foreach (var data in defaultEvents.FindAll((EventNodeData s) => (int)s.sourceRole == i)?.ToArray())
            {
                bool canTrigger = data.triggerConditions.EvaluateAll(data);
                if (canTrigger)
                {
                    EventInstance instance = GameObject.Instantiate(eventSlot, eventHolders[i]).GetComponent<EventInstance>();
                    instance.Initialize(data);
                    activeEvents.Add(instance);

                    Debug.Log($"[事件生成] 满足条件 → 创建事件：{data.eventName}");
                }
                else
                {
                    Debug.Log($"[事件保留] 条件不满足 → 保留事件：{data.eventName}");
                }
                if(GameManager.Instance.eventHolders[i].childCount>=3) break;
            }
        }
    }

    public void ProcessPlayerDefault()
    {
        if (GameManager.Instance.eventHolders[PLAYER_INDEX].childCount <= MAX_PLAYER_ACTION_COUNT)
        {
            foreach (var data in defaultEvents.FindAll((EventNodeData s) => (int)s.sourceRole == PLAYER_INDEX)?.ToArray())
            {
                bool canTrigger = data.triggerConditions.EvaluateAll(data);
                if (canTrigger)
                {
                    EventInstance instance = GameObject.Instantiate(actionSlot, eventHolders[PLAYER_INDEX]).GetComponent<EventInstance>();
                    instance.Initialize(data);
                    activeEvents.Add(instance);
                    Debug.Log($"[事件生成] 满足条件 → 创建事件：{data.eventName}");
                }
                else
                {
                    Debug.Log($"[事件保留] 条件不满足 → 保留事件：{data.eventName}");
                }
                if(GameManager.Instance.eventHolders[PLAYER_INDEX].childCount>=MAX_PLAYER_ACTION_COUNT) 
                {
                    break;
                }
            } 
        }
        GameManager.Instance.TransitionToState(TurnPhase.DrawCard);
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
                    GameObject.DestroyImmediate(evt.transform.gameObject);
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
                }
                GameObject.DestroyImmediate(evt.transform.gameObject);
                
            }
            
            HistoryLog.Log(evt, matched);
        }
        foreach(var child in GameManager.Instance.eventHolders[0].GetComponentsInChildren<RectTransform>())
        {
            Debug.Log($"{child.name}");
        }
        Debug.Log($"玩家槽位共{GameManager.Instance.eventHolders[0].childCount}个");
        GameManager.Instance.TransitionToState(TurnPhase.NPCAction);
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

