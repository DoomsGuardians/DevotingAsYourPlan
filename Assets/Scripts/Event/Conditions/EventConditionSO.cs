using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ConditionMode
{
    All, // 所有条件都满足
    Any  // 任意一个条件满足
}

public abstract class EventConditionSO : ScriptableObject
{
    public abstract bool Evaluate(EventContext context);
    public abstract string Description { get; }
}

