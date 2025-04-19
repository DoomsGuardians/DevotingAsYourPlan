using UnityEngine;

[CreateAssetMenu(menuName = "Events/Effects/ChangeStat")]
public class ChangeStatEffect : EventEffectSO
{
    public RoleType targetRole;
    public string statKey;
    public float valueChange;

    public override void Apply()
    {
        GameManager.Instance.GetRole(targetRole).AddStat(statKey, valueChange);
        Debug.Log($"[事件效果] {targetRole} 的 {statKey} 变化了 {valueChange}");
    }

    public override string Description => $"修改{targetRole}的{statKey}: {valueChange}";
}
