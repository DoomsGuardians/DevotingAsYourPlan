using JetBrains.Annotations;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Runtime.CompilerServices;

public class EventShowContainer : MonoBehaviour
{
    public static EventShowContainer Instance;

    [SerializeField] private CanvasGroup canvasGroup;

    [SerializeField] private RectTransform rectTransform;

    [SerializeField] private Vector2 inPos;

    [SerializeField] private Vector2 outPos;

    public RectTransform panel;             

    private void Awake() => Instance = this;

    public void PlayEventShowAnimation()
    {
        canvasGroup.DOFade(1, 1f);
        rectTransform.DOAnchorPos(inPos, .5f).SetEase(Ease.InQuad);
    }

    public void PlayEventHideAnimation()
    {
        canvasGroup.DOFade(0, 1f);
        rectTransform.DOAnchorPos(outPos, .5f).SetEase(Ease.OutQuad);
    }
    
    public void PlayCardShowAnimation()
    {
        canvasGroup.DOFade(1, .5f);
    }
    
    public void PlayCardHideAnimation()
    {
        canvasGroup.DOFade(0, .5f);
    }
}
