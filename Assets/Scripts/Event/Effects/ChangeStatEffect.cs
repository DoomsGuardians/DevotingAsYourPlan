using UnityEngine;
using System.Linq;
using UnityEngine.Serialization;

public enum ChangeMode
{
    Add,
    Set,
    Multiply
}

[CreateAssetMenu(menuName = "Events/Effects/属性处理")]
public class ChangeStatEffect : EventEffectSO
{
    [Header("目标角色")] public RoleType targetRole;

    [RoleStatKey] public string statKey;

    [Header("数值设置")] public ChangeMode mode = ChangeMode.Add;

    [Tooltip("基础值")] public float value = 1f;

    [Tooltip("波动范围（±X）")] public float variance = 0f;

    [FormerlySerializedAs("entryName")] [Tooltip("特定词条数量影响因子（1+词条数/卡牌总数）乘数")]
    public CardEntry specificEntry = null;

    [Tooltip("特定词条数量影响因子乘数 为0就没有效果哦")] public float entryactor = 1f;

    public override void Apply(EventInstance instance)
    {
        var role = GameManager.Instance.GetRole(targetRole);
        float appliedValue = (1 + rarityFactor * instance.RaritySum) * (value + Random.Range(-variance, variance));

        float finalFactor = 1;

        if (specificEntry != null && statKey == specificEntry.entryName)
        {
            finalFactor = entryactor * (1 + GameManager.Instance.playerCardHolder.cards.Count(c =>
                    c.runtimeData.entries.Any(e => e == specificEntry)) /
                GameManager.Instance.playerCardHolder.cards.Count());
        }

        switch (mode)
        {
            case ChangeMode.Add:
                role.AddStat(statKey, appliedValue * finalFactor);
                Debug.Log($"[事件效果] {targetRole} 的 {statKey} += {appliedValue * finalFactor:F2}");
                break;

            case ChangeMode.Set:
                role.SetStat(statKey, appliedValue * finalFactor);
                Debug.Log($"[事件效果] {targetRole} 的 {statKey} 设置为 {appliedValue * finalFactor:F2}");
                break;

            case ChangeMode.Multiply:
                float current = role.GetStat(statKey);
                float result = current * appliedValue * finalFactor;
                role.SetStat(statKey, result);
                Debug.Log($"[事件效果] {targetRole} 的 {statKey} *= {appliedValue * finalFactor:F2} → {result:F2}");
                break;
        }
    }

    public override string Description =>
        $"{targetRole} 的 {statKey} {(mode == ChangeMode.Add ? "+=" : (mode == ChangeMode.Set ? "设为" : "乘以"))} {entryactor} * " +
        (specificEntry ? $"{specificEntry.entryName}在玩家手牌中的占比 * " : "") + $"{value}" +
        (variance > 0 ? $" ±{variance}" : "");
}