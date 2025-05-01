using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using Cysharp.Threading.Tasks;

public class EndingPanel : MonoBehaviour
{
    public static EndingPanel Instance;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI tooltipText;
    [SerializeField] private Vector2 padding = new Vector2(16, 16); // 内边距（文本距离边框的距离）

    private void Awake() => Instance = this;

    private void Start()
    {
        Hide();
    }

    public async UniTask Show(string endingText)
    {
        if (endingText != "")
        {
            tooltipText.text = endingText;
            EventShowContainer.Instance.PlayEndingShowAnimation();
            await canvasGroup.DOFade(1f, 0.3f).SetEase(Ease.InSine).AsyncWaitForCompletion();
        }
    }
        

    public async UniTask Hide()
    {
        EventShowContainer.Instance.PlayCardHideAnimation();
        await canvasGroup.DOFade(0f, 0.3f).SetEase(Ease.OutSine).AsyncWaitForCompletion();
        //panel.gameObject.SetActive(false);
    }
    
}