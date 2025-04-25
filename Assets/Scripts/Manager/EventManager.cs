using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine.Assertions;
using System.Threading.Tasks;

public class EventManager
{
    
    public List<EventInstance> activeEvents;
    
    private List<EventNodeData> pendingEvents = new();

    private List<EventNodeData> defaultEvents = new();

    // 唯一事件的记录（触发过即加入）
    private HashSet<string> triggeredEventIDs = new();

    // 非唯一事件冷却：事件ID → 剩余冷却回合
    private Dictionary<string, int> cooldownTimers = new();

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
    public async UniTask ProcessEventTrigger()
    {
        await ProcessDefault();
        await ProcessPending();
        await GameManager.Instance.TransitionToStateAsync(TurnPhase.EndTurn);
    }
    private async UniTask ProcessPending()
    {
        if (GameManager.Instance.eventHolders[PLAYER_INDEX].childCount <= MAX_PLAYER_ACTION_COUNT)
        {
            foreach (var data in pendingEvents.FindAll((EventNodeData s) => (int)s.sourceRole == PLAYER_INDEX)?.ToArray())
            {
                // 是否唯一事件已触发
                if (data.isUnique && triggeredEventIDs.Contains(data.eventID))
                {
                    Debug.Log($"[事件跳过] 唯一事件已触发过：{data.eventName}");
                    continue;
                }

                // 是否在冷却中
                if (cooldownTimers.TryGetValue(data.eventID, out int cooldown) && cooldown > 0)
                {
                    Debug.Log($"[事件跳过] {data.eventName} 冷却中，剩余 {cooldown} 回合");
                    continue;
                }

                // 如果事件有冷却时间且没有加入到 cooldownTimers 中，就加入
                if (data.cooldownTurns > 0 && !cooldownTimers.ContainsKey(data.eventID))
                {
                    cooldownTimers[data.eventID] = data.cooldownTurns; // 初始化冷却时间
                    Debug.Log($"[冷却设置] {data.eventName} 设置初始冷却 {data.cooldownTurns} 回合");
                }

                bool canTrigger = data.triggerConditions.EvaluateAll(data);
                if (canTrigger)
                {
                    EventInstance instance = GameObject.Instantiate(actionSlot, eventHolders[PLAYER_INDEX]).GetComponent<EventInstance>();
                    await instance.Initialize(data);
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
                // 是否唯一事件已触发
                if (data.isUnique && triggeredEventIDs.Contains(data.eventID))
                {
                    Debug.Log($"[事件跳过] 唯一事件已触发过：{data.eventName}");
                    continue;
                }

                // 是否在冷却中
                if (cooldownTimers.TryGetValue(data.eventID, out int cooldown) && cooldown > 0)
                {
                    Debug.Log($"[事件跳过] {data.eventName} 冷却中，剩余 {cooldown} 回合");
                    continue;
                }

                // 如果事件有冷却时间且没有加入到 cooldownTimers 中，就加入
                if (data.cooldownTurns > 0 && !cooldownTimers.ContainsKey(data.eventID))
                {
                    cooldownTimers[data.eventID] = data.cooldownTurns; // 初始化冷却时间
                    Debug.Log($"[冷却设置] {data.eventName} 设置初始冷却 {data.cooldownTurns} 回合");
                }

                bool canTrigger = data.triggerConditions.EvaluateAll(data);
                if (canTrigger)
                {
                    EventInstance instance = GameObject.Instantiate(eventSlot, eventHolders[i]).GetComponent<EventInstance>();
                    await instance.Initialize(data);
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
    private async UniTask ProcessDefault()
    {
        //处理其他角色
        for (int i = PLAYER_INDEX+1; i <= GameManager.Instance.eventHolders.Count-1 ; i++)
        {
            if (GameManager.Instance.eventHolders[i].childCount >= 3) continue;
            foreach (var data in defaultEvents.FindAll((EventNodeData s) => (int)s.sourceRole == i)?.ToArray())
            {
                // 是否唯一事件已触发
                if (data.isUnique && triggeredEventIDs.Contains(data.eventID))
                {
                    Debug.Log($"[事件跳过] 唯一事件已触发过：{data.eventName}");
                    continue;
                }

                // 是否在冷却中
                if (cooldownTimers.TryGetValue(data.eventID, out int cooldown) && cooldown > 0)
                {
                    Debug.Log($"[事件跳过] {data.eventName} 冷却中，剩余 {cooldown} 回合");
                    continue;
                }

                // 如果事件有冷却时间且没有加入到 cooldownTimers 中，就加入
                if (data.cooldownTurns > 0 && !cooldownTimers.ContainsKey(data.eventID))
                {
                    cooldownTimers[data.eventID] = data.cooldownTurns; // 初始化冷却时间
                    Debug.Log($"[冷却设置] {data.eventName} 设置初始冷却 {data.cooldownTurns} 回合");
                }

                bool canTrigger = data.triggerConditions.EvaluateAll(data);
                if (canTrigger)
                {
                    EventInstance instance = GameObject.Instantiate(eventSlot, eventHolders[i]).GetComponent<EventInstance>();
                    await instance.Initialize(data);
                    activeEvents.Add(instance);

                    Debug.Log($"[事件生成] 满足条件 -> 创建事件：{data.eventName}");
                }
                else
                {
                    Debug.Log($"[事件保留] 条件不满足 -> 保留事件：{data.eventName}");
                }
                if(GameManager.Instance.eventHolders[i].childCount>=3) break;
            }
        }
    }

    public async UniTask ProcessPlayerDefault()
    {
        if (GameManager.Instance.eventHolders[PLAYER_INDEX].childCount <= MAX_PLAYER_ACTION_COUNT)
        {
            foreach (var data in defaultEvents.FindAll((EventNodeData s) => (int)s.sourceRole == PLAYER_INDEX)?.ToArray())
            {
                // 是否唯一事件已触发
                if (data.isUnique && triggeredEventIDs.Contains(data.eventID))
                {
                    Debug.Log($"[事件跳过] 唯一事件已触发过：{data.eventName}");
                    continue;
                }

                // 是否在冷却中
                if (cooldownTimers.TryGetValue(data.eventID, out int cooldown) && cooldown > 0)
                {
                    Debug.Log($"[事件跳过] {data.eventName} 冷却中，剩余 {cooldown} 回合");
                    continue;
                }

                // 如果事件有冷却时间且没有加入到 cooldownTimers 中，就加入
                if (data.cooldownTurns > 0 && !cooldownTimers.ContainsKey(data.eventID))
                {
                    cooldownTimers[data.eventID] = data.cooldownTurns; // 初始化冷却时间
                    Debug.Log($"[冷却设置] {data.eventName} 设置初始冷却 {data.cooldownTurns} 回合");
                }

                bool canTrigger = data.triggerConditions.EvaluateAll(data);
                if (canTrigger)
                {
                    EventInstance instance = GameObject.Instantiate(actionSlot, eventHolders[PLAYER_INDEX]).GetComponent<EventInstance>();
                    await instance.Initialize(data);
                    activeEvents.Add(instance);
                    Debug.Log($"[事件生成] 满足条件 -> 创建事件：{data.eventName}");
                }
                else
                {
                    Debug.Log($"[事件保留] 条件不满足 -> 保留事件：{data.eventName}");
                }
                if(GameManager.Instance.eventHolders[PLAYER_INDEX].childCount>=MAX_PLAYER_ACTION_COUNT) 
                {
                    break;
                }
            } 
        }
        await GameManager.Instance.TransitionToStateAsync(TurnPhase.DrawCard);
    }

    // 每回合递减冷却时间
    public void TickCooldowns()
    {
        List<string> cooldownKeys = new List<string>(cooldownTimers.Keys);
        foreach (string eventID in cooldownKeys)
        {
            if (cooldownTimers[eventID] > 0)
            {
                cooldownTimers[eventID]--; // 减少冷却时间
                Debug.Log($"[冷却] {eventID} 冷却中，剩余 {cooldownTimers[eventID]} 回合");
            }

            // 如果冷却结束，移除事件
            if (cooldownTimers[eventID] <= 0)
            {
                cooldownTimers.Remove(eventID);
                Debug.Log($"[冷却完成] {eventID} 冷却结束");
            }
        }
    }

    #endregion
    
    #region 处理事件效果
    public async UniTask ResolveEventsEffectAsync()
    {
        foreach (var evt in activeEvents.ToArray())
        {
            if (evt.resolved) continue;

            bool matched = false;

            // 遍历所有结果分支
            foreach (var branch in evt.data.outcomeBranches)
            {
                if (branch.matchConditions.EvaluateAll(evt))
                {
                    Debug.Log($"[事件处理] 【{evt.data.eventName}】匹配分支【{branch.label}】");

                    await ExecuteEffectsAsync(evt, branch.effects);
                    matched = true;

                    await CleanupEventAsync(evt);
                    break;
                }
            }

            // 没有匹配分支，但事件已过期
            if (!matched && evt.IsExpired())
            {
                Debug.Log($"[事件处理] 【{evt.data.eventName}】过期未匹配任何分支");

                await ExecuteEffectsAsync(evt, evt.data.expiredEffects);
            
                await CleanupEventAsync(evt);
            }
            else if (!matched)
            {
                Debug.LogWarning($"[事件处理] 【{evt.data.eventName}】没有匹配任何分支！");
            }

            HistoryLog.Log(evt, matched);
        }
    }

    public async UniTask CleanupEventAsync(EventInstance evt)
    {
        // 卡牌转移给玩家
        evt.ToggleCardShow(true);
        var cards = new List<Card>(evt.cardHolder.cards);
        for (int i = cards.Count - 1; i >= 0; i--)
        {
            evt.cardHolder.RemoveCard(cards[i]);
            GameManager.Instance.playerCardHolder.TransferCard(cards[i]);
        }

        // 销毁事件之前，确保动画播放完成
        await evt.PlayAndDestroyAfterAnim();
        Debug.Log($"[事件处理] 清理【{evt.data.eventName}】过期分支");
        activeEvents.Remove(evt);

        // 这部分会等到动画播放完再销毁事件
        GameObject.Destroy(evt.gameObject);
    }


    public async UniTask ExecuteEffectsAsync(EventInstance evt, List<EventEffectSO>effects)
    {
        foreach (var effect in effects)
        {
            await effect.ApplyAsync(evt); // 统一调用 async，自动适配新旧效果
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

    #region 功能函数

    public void RemoveEventFromPool(EventInstance evt)
    {
        pendingEvents.Remove(evt.data);
        defaultEvents.Remove(evt.data);
        cooldownTimers.Remove(evt.data.eventID);
        triggeredEventIDs.Remove(evt.data.eventID);
    }

    #endregion

}

