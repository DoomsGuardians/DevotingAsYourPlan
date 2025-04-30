using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/分支")]
public class EventOutcomeBranch : ScriptableObject
{
    [Tooltip("表示分支标签名称")] public string label; // 用于调试：如 "劳动力处理"
    [Tooltip("结算事件条件组")] public ResolveConditionGroup matchConditions; // 对 holder 中的卡片进行判断
    [Tooltip("结算事件效果")] public List<EventEffectSO> effects; // 如果匹配，接下来的事件链
}