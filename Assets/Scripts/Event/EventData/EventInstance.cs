using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Animancer;
using UnityEngine.EventSystems;

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

