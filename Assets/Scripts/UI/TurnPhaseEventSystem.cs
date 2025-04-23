using System;
using UnityEngine;

public static class TurnPhaseEventSystem
{
    public static event Action OnPhaseChanged;

    public static void RaisePhaseChanged(TurnPhase newPhase)
    {
        //Debug.Log($"[TurnPhaseEventSystem] 当前回合切换到：{newPhase}");
        OnPhaseChanged?.Invoke();
    }
}
