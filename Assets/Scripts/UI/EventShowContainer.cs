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

    public  void PlayEventShowAnimation()
    {
        canvasGroup.DOFade(1, 1.5f);
        rectTransform.DOAnchorPos(inPos, 1f).SetEase(Ease.InQuad);
    }

    public  void PlayEventHideAnimation()
    {
        canvasGroup.DOFade(0, 1.5f);
        rectTransform.DOAnchorPos(outPos, 1f).SetEase(Ease.OutSine);
    }
}
