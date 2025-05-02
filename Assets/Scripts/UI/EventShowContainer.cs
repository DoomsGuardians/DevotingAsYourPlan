using JetBrains.Annotations;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Runtime.CompilerServices;

public class EventShowContainer : MonoBehaviour
{
    public static EventShowContainer Instance;

    [SerializeField] private CanvasGroup canvasGroup;

    [SerializeField] private RectTransform continueRectTransform;

    [SerializeField] private RectTransform returnRectTransform;

    [SerializeField] private Vector2 inPos;

    [SerializeField] private Vector2 outPos;

    public RectTransform panel;             

    private void Awake() => Instance = this;

    public void PlayEventShowAnimation()
    {
        canvasGroup.DOFade(1, 1f);
        continueRectTransform.DOAnchorPos(inPos, .5f).SetEase(Ease.InQuad);
    }

    public void PlayEventHideAnimation()
    {
        canvasGroup.DOFade(0, 1f);
        continueRectTransform.DOAnchorPos(outPos, .5f).SetEase(Ease.OutQuad);
    }
    
    public void PlayCardShowAnimation()
    {
        canvasGroup.DOFade(1, .5f);
    }
    
    public void PlayCardHideAnimation()
    {
        canvasGroup.DOFade(0, .5f);
    }
    
    public void PlayEndingShowAnimation()
    {
        canvasGroup.DOFade(1, .5f);
        returnRectTransform.DOAnchorPos(inPos, .5f).SetEase(Ease.InQuad);
    }
    
    public void PlayEndingHideAnimation()
    {
        canvasGroup.DOFade(0, .5f);
        returnRectTransform.DOAnchorPos(outPos, .5f).SetEase(Ease.OutQuad);
    }
}
