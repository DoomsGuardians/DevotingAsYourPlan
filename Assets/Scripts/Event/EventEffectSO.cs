using UnityEngine;

public abstract class EventEffectSO : ScriptableObject, IEventEffect
{
    public abstract void Apply();
    public abstract string Description { get; }
}

