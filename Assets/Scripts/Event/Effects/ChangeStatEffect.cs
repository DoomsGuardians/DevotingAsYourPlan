using UnityEngine;

public enum ChangeMode { Add, Set, Multiply }

[CreateAssetMenu(menuName = "Events/Effects/属性处理")]
public class ChangeStatEffect : EventEffectSO
{
    [Header("目标角色")]
    public RoleType targetRole;

    [RoleStatKey]
    public string statKey;

    [Header("数值设置")]
    public ChangeMode mode = ChangeMode.Add;

    [Tooltip("基础值")]
    public float value = 1f;

    [Tooltip("波动范围（±X）")]
    public float variance = 0f;

    public override void Apply()
    {
        var role = GameManager.Instance.GetRole(targetRole);
        float appliedValue = value + Random.Range(-variance, variance);

        switch (mode)
        {
            case ChangeMode.Add:
                role.AddStat(statKey, appliedValue);
                Debug.Log($"[事件效果] {targetRole} 的 {statKey} += {appliedValue:F2}");
                break;

            case ChangeMode.Set:
                role.SetStat(statKey, appliedValue);
                Debug.Log($"[事件效果] {targetRole} 的 {statKey} 设置为 {appliedValue:F2}");
                break;

            case ChangeMode.Multiply:
                float current = role.GetStat(statKey);
                float result = current * appliedValue;
                role.SetStat(statKey, result);
                Debug.Log($"[事件效果] {targetRole} 的 {statKey} *= {appliedValue:F2} → {result:F2}");
                break;
        }
    }

    public override string Description =>
        $"{targetRole} 的 {statKey} {(mode == ChangeMode.Add ? "+=" : (mode == ChangeMode.Set ? "设为" : "乘以"))} {value}" +
        (variance > 0 ? $" ±{variance}" : "");
}
