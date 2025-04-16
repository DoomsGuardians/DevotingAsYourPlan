using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EventOutcomeBranch
{
    public string label; // 用于调试：如 "劳动力处理"
    public List<EventConditionSO> matchConditions; // 对 holder 中的卡片进行判断
    public List<EventEffectSO> effects;
    public List<EventTransition> transitions; // 如果匹配，接下来的事件链
}
