using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;

public class LaborStatCalculateModule : IStatCalculatorModule
{
    public float Calculate(Dictionary<RoleType, Role> roles)
    {
        float S = roles[RoleType.People].GetStat("信众数");
        float O = roles[RoleType.People].GetStat("顺从正统度");
        float M = roles[RoleType.Player].GetStat("神秘性");
        float D = roles[RoleType.Player].GetStat("神格性");
        float F = Mathf.Max(0, roles[RoleType.People].GetStat("粮储")/roles[RoleType.People].GetStat("人口数")-1f) ;
        
        return Mathf.Clamp( Mathf.Floor(  Mathf.Log(S / 50f + 1f) * (1f + O/200f + M/200f + D/300f + F * 1.5f) ), 0f, 6f ); 
    }
}