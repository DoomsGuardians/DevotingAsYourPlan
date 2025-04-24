using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public static class InputUtility
{
    public static async UniTask WaitForClickAsync()
    {
        // 无限循环，直到玩家点击
        while (!Input.GetMouseButtonDown(0)) // 0 是左键，1 是右键，2 是中键
        {
            await UniTask.Yield(); // 这里是等待，直到下一帧
        }
    }
}
