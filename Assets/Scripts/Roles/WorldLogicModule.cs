using UnityEngine;
using System.Collections.Generic;

public class WorldLogicModule : IRoleLogicModule
{
    public void Settle(Role role, int round)
    {
        float boomBane = role.GetStat("灾丰积累度");
        float lastOutput = GameManager.Instance.GetRole(RoleType.People).GetStat("粮储");
        List<float> foodHistory = GameManager.Instance.GetRole(RoleType.People).GetStatHistory("粮储");
        float prevOutput = foodHistory.Count > 0 ? foodHistory[^1] : GameManager.Instance.GetRole(RoleType.People).GetStat("粮储");;
        float population = GameManager.Instance.GetRole(RoleType.People).GetStat("人口数");
        float mystery = role.GetStat("神秘性");
        float animacy = role.GetStat("神格性");

        // 灾丰积累度
        float fluctuation = (lastOutput - prevOutput) / Mathf.Max(1f, population) * 200f;
        float cycle = Mathf.Sin(round / 8f) * 10f;
        float mysteryMod = mystery * 0.02f;
        float animacyMode = animacy * 0.02f;
        float random = Random.Range(-3f, 3f);

        boomBane += fluctuation + cycle + mysteryMod + random;
        boomBane = Mathf.Clamp(boomBane, -100f, 100f);
        role.SetStat("灾丰积累度", boomBane);

        // 动荡度
        float unrest = role.GetStat("动荡度");
        int unrestChange = Random.Range(-5, 5);
        if (role.GetStat("orthodox") < 0) unrestChange += Random.Range(1, 3);
        if (Mathf.Abs(boomBane) > 50) unrestChange += 1;
        if (round % 10 == 0) unrestChange += 1;

        unrest = Mathf.Clamp(unrest + unrestChange, 0, 100);
        role.SetStat("动荡度", unrest);
    }
}