using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;
using TMPro;
using Cysharp.Threading.Tasks;

public class TurnStateHandler : MonoBehaviour
{
    public static TurnStateHandler Instance;
    
    private void Awake() => Instance = this;
    
    [SerializeField] private AnimancerComponent animancerComponent;
    [SerializeField] private TMP_Text round;
    [SerializeField] private TMP_Text desc;

    [SerializeField] private ClipTransition clip;

    public async UniTask TurnStateAnim(string round, string desc)
    {
        this.round.text = round;
        this.desc.text = desc;
        await animancerComponent.Play(clip);
        await animancerComponent.Stop(clip);
        // await UniTask.Delay((int)(clip.Clip.length * 1000-500)); 
    }
}
