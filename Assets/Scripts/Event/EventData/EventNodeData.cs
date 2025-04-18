using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/Event Node")]
public class EventNodeData : ScriptableObject
{
    [Tooltip("事件ID")]
    public string eventID;
    [Tooltip("事件名字")]
    public string eventName;
    [Tooltip("事件是由那个角色发出的")]
    public RoleType sourceRole;
    [Tooltip("设置触发事件的条件组")]
    public TriggerConditionGroup triggerConditions; // 条件组
    [Tooltip("设置事件根据不同输入卡牌的分支条件和分支效果组")]
    public List<EventOutcomeBranch> outcomeBranches;
    [Tooltip("设置事件过期未处理的效果组")]
    public List<EventEffectSO> expiredEffects; // 过期事件
    
    [TextArea]
    public string description;
    public Sprite icon;
    public int duration = 2;
}
