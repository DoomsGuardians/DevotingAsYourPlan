using UnityEngine;

[CreateAssetMenu(menuName = "Events/Conditions/Trigger Conditions/回合数范围")]
public class TurnCountTriggerCondition : EventTriggerConditionSO
{
    [Tooltip("允许触发的最小回合（包含）")] public int minTurn = 1;

    [Tooltip("允许触发的最大回合（包含）")] public int maxTurn = 999;

    public override bool Evaluate(EventNodeData context)
    {
        int currentTurn = GameManager.Instance.turnStateMachine.TurnNum;

        bool result = currentTurn >= minTurn && currentTurn <= maxTurn;
        Debug.Log($"[回合触发] 当前回合 {currentTurn}, 允许范围 [{minTurn}, {maxTurn}] -> {(result ? "满足" : "不满足")}");
        return result;
    }

    public override string Description => $"回合数 屬於 [{minTurn}, {maxTurn}]";
}