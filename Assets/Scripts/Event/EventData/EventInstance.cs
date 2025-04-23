using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Animancer;
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks;

public class EventInstance : MonoBehaviour
{
    [Header("Runtime赋值")]
    public EventNodeData data;
    public int remainingLife;
    public bool resolved;
    public Role sourceRole;

    [Header("UI组件")] 
    
    public HorizontalCardHolder cardHolder;
    
    [SerializeField] private TMP_Text titleText;

    [SerializeField] private TMP_Text descText;

    [SerializeField] private TMP_Text remainingLifeText;

    [SerializeField] private Image illust;

    [SerializeField] public AnimancerComponent animancer;

    [SerializeField] public List<ClipTransition> clips;
    

    public void Initialize(EventNodeData data)
    {
        this.data = data;
        this.name = data.eventName;
        this.sourceRole = GameManager.Instance.GetRole(data.sourceRole);
        remainingLife = data.duration;
        if (titleText)
        {
            titleText.text = data.eventName;
        }
        if(descText)
        {
            descText.text = data.description;
        }

        if (remainingLifeText)
        {
            remainingLifeText.text = $"剩余{remainingLife}年";
        }

        if (illust && data.icon)
        {
            illust.sprite = data.icon;
        }
        
        PlayEntryAnimation();
    }
    
    /// 播放入场动画
    private async void PlayEntryAnimation()
    {
        if (animancer != null && clips != null && clips.Count > 0)
        {
            // 播放第一个入场动画
            var state = animancer.Play(clips[2]);
            // 等待动画播放完毕
            await UniTask.Delay((int)(clips[2].Clip.length * 1000+500)); ; // 等待动画播放完成
            Debug.Log("[EventInstance] 入场动画播放完毕");
        }
        else
        {
            Debug.LogWarning("[EventInstance] 没有入场动画！");
        }
    }
    
    public void ToggleCardShow(bool value)
    {
        cardHolder.ToggleShow(value);
    }
    
    public void TickLife()
    {
        remainingLife--;
        if (remainingLifeText)
        {
            remainingLifeText.text = $"剩余{remainingLife}年";
        }
    }

    public bool IsExpired() => remainingLife <= 0;
}

