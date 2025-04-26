using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
    public static Tooltip Instance;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform panel;       
    [SerializeField] private TextMeshProUGUI tooltipTitle; // Tooltip 背景面板
    [SerializeField] private TextMeshProUGUI tooltipText; 
    [SerializeField] private TextMeshProUGUI tooltipRemainingLife;// Tooltip 文本内容
    [SerializeField] private Image cardImage;
    [SerializeField] private Vector2 padding = new Vector2(16, 16); // 内边距（文本距离边框的距离）

    private void Awake() => Instance = this;

    private void Start()
    {
        Hide();
    }

    public async void Show(CardRuntime cardRuntime)
    {
        if(cardRuntime == null)return;
        tooltipText.text = cardRuntime.data.description;
        tooltipTitle.text = cardRuntime.data.cardName;
        tooltipRemainingLife.text = $"{cardRuntime.remainingLife}";
        cardImage.sprite = cardRuntime.data.illustration;
        // 更新 Tooltip 面板的大小，使其适应文本内容
        UpdateTooltipSize();

        // 显示面板
        //panel.gameObject.SetActive(true);
        EventShowContainer.Instance.PlayCardShowAnimation();
        await canvasGroup.DOFade(1f, 0.3f).SetEase(Ease.InSine).AsyncWaitForCompletion();
    }

    public async void Hide()
    {
        EventShowContainer.Instance.PlayCardHideAnimation();
        await canvasGroup.DOFade(0f, 0.3f).SetEase(Ease.OutSine).AsyncWaitForCompletion();
        //panel.gameObject.SetActive(false);
    }

    // 根据文本内容调整面板大小
    private void UpdateTooltipSize()
    {
        // 获取文本的首选尺寸
        Vector2 preferredSize = tooltipText.GetPreferredValues();

        // 考虑 padding 来计算最终的面板尺寸
        float heightWithPadding = Mathf.Min(preferredSize.y + padding.y + tooltipText.fontSize, 100f) ;

        // 计算面板的大小
        panel.sizeDelta = new Vector2(panel.sizeDelta.x, heightWithPadding);

        // 确保文本不会溢出面板
        tooltipText.enableWordWrapping = true; // 开启自动换行
        tooltipText.overflowMode = TextOverflowModes.Overflow; // 防止文本溢出
    }
}