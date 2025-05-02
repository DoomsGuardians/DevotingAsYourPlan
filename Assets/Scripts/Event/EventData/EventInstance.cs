using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Animancer;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Linq;
using Cysharp.Threading.Tasks.Triggers;

public class EventInstance : MonoBehaviour
{
    [Header("Runtime赋值")]
    public EventNodeData data;
    public int remainingLife;
    public bool resolved;
    public Role sourceRole;
    private bool isFold = true;

    [Header("UI组件")] 
    
    [SerializeField] RectTransform rectTransform;

    public HorizontalCardHolder cardHolder;
    
    [SerializeField] private TMP_Text titleText;

    [SerializeField] private TMP_Text descText;

    [SerializeField] private TMP_Text remainingLifeText;

    [SerializeField] private Image illust;

    [SerializeField] public AnimancerComponent animancer;

    [SerializeField] public List<ClipTransition> clips;
    [HideInInspector] public List<CardRuntime> originalCards = new();
    
    [HideInInspector] public int RaritySum
    {
        get
        {
            return cardHolder.cards.Sum(card => card.runtimeData.data.rarity);
        }
    }

    public async UniTask Initialize(EventNodeData data)
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
        AudioManager.Instance.PlaySFX("event_in");
        await PlayEntryAnimation();
    }

    public async UniTask PlayAndDestroyAfterAnim()
    {
        if (animancer != null && clips != null && clips.Count > 0)
        {

               var clipIndex = Mathf.Min(3, clips.Count - 1);  // 只使用有效的索引
                var clip = clips[clipIndex];

                if (clip != null)
                {
                    AudioManager.Instance.PlaySFX("event_done");
                    // 播放动画
                    await animancer.Play(clip);
                    await animancer.Stop(clip);
                }
                else
                {
                    Debug.LogError("[EventInstanceExtensions] 无效的动画剪辑");
                } 
            
        }
        else
        {
            Debug.LogError("[EventInstanceExtensions] Animancer 或动画剪辑列表为空");
        }
    }

    public async UniTask ShowEnd(string name, string desc, Sprite img)
    {
        if(clips[4] == null || clips[4] == null)
        {
            return;
        }
        EventShowContainer.Instance.PlayEventShowAnimation();

        await animancer?.Play(clips[4]);
        animancer?.Stop(clips[4]);

        
        this.name = name;
        if (titleText && name != null)
        {
            titleText.text = name;
        }
        if(descText && desc!=null)
        {
            descText.text = desc;
        }

        if (illust && img)
        {
            illust.sprite = img;
        }

        // 记录当前 UI 元素的世界位置
        rectTransform.SetParent(EventShowContainer.Instance.panel, true);
        transform.localScale = Vector3.one;
        rectTransform.pivot = new Vector2(.5f, .5f);
        rectTransform.DOScale(new Vector3(2f, 2f, 1f), .5f).SetEase(Ease.InQuad);
        animancer?.Play(clips[5]);
        AudioManager.Instance.PlaySFX("turn_transition");
        await rectTransform.DOLocalMove(Vector3.zero, .5f).SetEase(Ease.InQuad).AsyncWaitForCompletion();
        await InputUtility.WaitForClickAsync();
        EventShowContainer.Instance.PlayEventHideAnimation();
    }
    
    /// 播放入场动画
    private async UniTask PlayEntryAnimation()
    {
        if (animancer != null && clips != null && clips.Count > 0)
        {
            // 播放第一个入场动画
            await animancer.Play(clips[2]);
            // 等待动画播放完毕
            //await UniTask.Delay((int)(clips[2].Clip.length * 1000-200)); // 等待动画播放完成
            await animancer.Stop(clips[2]);
        }
        else
        {
            Debug.LogWarning("[EventInstance] 没有入场动画！");
        }
    }
    
    public async UniTask ToggleCardShow(bool value)
    {
        if(isFold == value) return;
        isFold = value;
        Debug.Log($"变成了{value}");
        if (animancer != null)
        {
            if (isFold)
            {
                AudioManager.Instance.PlaySFX("event_close");
                animancer.Play(clips[0]);
                await UniTask.Delay((int)(clips[0].Clip.length * 700));
                cardHolder.ToggleShow(value);
                cardHolder.isFold = isFold;
                //await animancer.Stop(clips[0]);
            }
            else
            {
                AudioManager.Instance.PlaySFX("event_open");
                cardHolder.ToggleShow(value);
                cardHolder.isFold = isFold;
                await animancer.Play(clips[1]);
                //await animancer.Stop(clips[1]);
            }
        }    
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

