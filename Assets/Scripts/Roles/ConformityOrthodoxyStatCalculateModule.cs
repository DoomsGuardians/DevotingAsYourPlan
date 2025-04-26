using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;

public class ConformityOrthodoxyStatCalculateModule : IStatCalculatorModule
{
    public float Calculate(Dictionary<RoleType, Role> roles)
    {
        return (Mathf.Abs(roles[RoleType.Player].GetStat("神秘性") / 200) + Mathf.Abs(roles[RoleType.Player].GetStat("神格性") / 200)+1) * roles[RoleType.People].GetStat("顺从正统度") / 200 ; //返回一个范围-1~1的值，-1表示高概率异端，1表示高概率正统
    }
}