using UnityEngine;

[CreateAssetMenu(menuName = "Events/Effects/基于历史幅度增减")]
public class ChangeStatByDeltaEffect : EventEffectSO
{
    public RoleType targetRole;

    [RoleStatKey]
    public string statKey;

    [Tooltip("乘以变化值的倍数")]
    public float multiplier = 1f;

    [Tooltip("是否取反趋势（例如下降变为正值）")]
    public bool invertDelta = false;

    [Tooltip("无历史记录时跳过（否则使用 0 作为前值）")]
    public bool skipIfNoHistory = true;

    public override void Apply()
    {
        var role = GameManager.Instance.GetRole(targetRole);
        var history = role.GetStatHistory(statKey);

        if (history == null || history.Count < 2)
        {
            if (skipIfNoHistory)
            {
                Debug.LogWarning($"[变化增减] {statKey} 无足够历史记录，跳过");
                return;
            }
            else
            {
                history.Insert(0, 0); // 使用 0 作为默认前值
            }
        }

        float current = history[^1];       // 当前值
        float previous = history[^2];      // 上一回合值
        float delta = current - previous;

        if (invertDelta) delta = -delta;

        float change = delta * multiplier;
        role.AddStat(statKey, change);

        Debug.Log($"[变化增减] {targetRole} 的 {statKey} 从 {previous} → {current}，Δ={delta:F2} × {multiplier} = {change:F2}");
    }

    public override string Description =>
        $"根据 {targetRole} 的 {statKey} 变化值 × {multiplier}" +
        (invertDelta ? "（取反）" : "") +
        (skipIfNoHistory ? "（无记录跳过）" : "（无记录视为0）");
}
