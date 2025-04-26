using UnityEngine;
using System.Collections.Generic;

public class TributeStatCalculateModule :IStatCalculatorModule, IRarityCalculatorModule
{
    public float Calculate(Dictionary<RoleType, Role> roles)
    {
        float P = roles[RoleType.Player].GetStat("信奉度");
        float S = roles[RoleType.People].GetStat("信众数");
        float O = roles[RoleType.People].GetStat("顺从正统度");
        float M = roles[RoleType.Player].GetStat("神秘性");
        float D = roles[RoleType.Player].GetStat("神格性");
        float F = Mathf.Max(0, roles[RoleType.People].GetStat("粮储")/roles[RoleType.People].GetStat("人口数")-1f) ;
        float E = roles[RoleType.World].GetStat("异象潮位");
        float Dev = roles[RoleType.People].GetStat("发展度");   
        return Mathf.Clamp( Mathf.Floor( (Mathf.Log(S / 100f +1f) )* (1f + P / 100f) * (1 - M / 100f) * (1 + D / 100f) * (1 + O / 100f) * (1 + E / 100f) * (1f + F * 0.5f)  * (1 + Dev / 150f)), 0f, 5f ); 
    }

    public Dictionary<int, int> CalculateRarity (Dictionary<RoleType, Role> roles)
    {
        float P = roles[RoleType.Player].GetStat("信奉度");
        float S = roles[RoleType.People].GetStat("信众数");
        float O = roles[RoleType.People].GetStat("顺从正统度");
        float M = roles[RoleType.Player].GetStat("神秘性");
        float D = roles[RoleType.Player].GetStat("神格性");
        float E = roles[RoleType.World].GetStat("异象潮位");
        float Dev = roles[RoleType.People].GetStat("发展度");

        int commonWeight = Mathf.RoundToInt(60f + (P * 0.1f) - (M * 0.05f) + (D * 0.03f) + (O * 0.02f) + (E * 0.05f) - (Dev * 0.1f));
        int rareWeight = Mathf.RoundToInt(30f + (P * 0.05f) + (M * 0.03f) + (D * 0.02f) + (O * 0.015f) + (Dev * 0.05f) + (E * 0.03f));
        int sacredWeight = Mathf.RoundToInt(8f + (P * 0.03f) + (M * 0.02f) + (D * 0.02f) + (O * 0.01f) + (E * 0.02f) + (Dev * 0.03f));
        int miracleWeight = Mathf.RoundToInt(2f + (P * 0.01f) + (M * 0.01f) + (D * 0.01f) + (O * 0.005f) + (E * 0.02f) + (Dev * 0.02f));

        return new Dictionary<int, int>()
        {
            { 0, commonWeight },
            { 1, rareWeight },
            { 2, sacredWeight },
            { 3, miracleWeight }
        };
    }
}