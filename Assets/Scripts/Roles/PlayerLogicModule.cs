using UnityEngine;

public class PlayerLogicModule : IRoleLogicModule
{
    public void Settle(Role role, int round)
    {
        float health = role.GetStat("健康度");
        role.SetStat("健康度", Mathf.Max(0, health - 1));

        // 更多“信奉度”“神秘性”“神格性”的事件逻辑写在事件系统中影响它们即可
    }
}