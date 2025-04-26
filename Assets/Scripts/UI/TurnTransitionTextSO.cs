using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "回合宣言配置", menuName = "Turn/回合宣言")]
public class TurnTransitionTextSO : ScriptableObject
{
    [Tooltip("回合开始宣言")]
    public List<string> TurnStrings;

    [Tooltip("开始年份")]
    public int startYear = 0;

    [Tooltip("开始年龄")]
    public int startAge = 0;

    // 在 Inspector 中修改任何属性时自动调用
    private void OnValidate()
    {
        // 验证 TurnStrings 数组是否包含 12 个元素
        if (TurnStrings.Count != 12)
        {
            Debug.LogWarning("TurnStrings 数组长度必须为 12！");
        }
    }
}
//"", "", "奉献获得", "新章开篇", "", "", "处理事件", "呈现结果", "羊群周游", "世界运转", "", ""
