using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Mono.CSharp;

public class SettingBtn : MonoBehaviour
{
    public async UniTask OnOptionsPressed()
    {
        OptionsMenu.Instance.Show();
        await UniTask.Yield();
    }
}
