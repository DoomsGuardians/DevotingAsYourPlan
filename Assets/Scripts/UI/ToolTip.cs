using System;
using TMPro;
using UnityEngine;

public class Tooltip : MonoBehaviour
{
    public static Tooltip Instance;

    [SerializeField] private RectTransform panel;               // Tooltip 背景面板
    [SerializeField] private TextMeshProUGUI tooltipText;        // Tooltip 文本内容
    [SerializeField] private Vector2 padding = new Vector2(16, 16); // 内边距（文本距离边框的距离）

    private void Awake() => Instance = this;

    private void Start()
    {
        Hide();
    }

    private void Update()
    {
        if (panel.gameObject.activeSelf)
            FollowMouse();
    }

    public void Show(string content)
    {
        tooltipText.text = content;

        // 更新 Tooltip 面板的大小，使其适应文本内容
        UpdateTooltipSize();

        // 显示面板
        panel.gameObject.SetActive(true);
        FollowMouse();
    }

    public void Hide() => panel.gameObject.SetActive(false);

    // 根据文本内容调整面板大小
    private void UpdateTooltipSize()
    {
        // 获取文本的首选尺寸
        Vector2 preferredSize = tooltipText.GetPreferredValues();

        // 考虑 padding 来计算最终的面板尺寸
        float widthWithPadding = preferredSize.x + padding.x;
        float heightWithPadding = preferredSize.y + padding.y;

        // 计算面板的大小
        panel.sizeDelta = new Vector2(widthWithPadding, heightWithPadding);

        // 确保文本不会溢出面板
        tooltipText.enableWordWrapping = true; // 开启自动换行
        tooltipText.overflowMode = TextOverflowModes.Overflow; // 防止文本溢出
    }

    // 面板跟随鼠标
    private void FollowMouse()
    {
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)panel.parent, Input.mousePosition, null, out pos);

        // 微调位置，避免遮挡鼠标
        float xOffset = 10f; // X 轴的偏移量
        float yOffset = -10f; // Y 轴的偏移量

        // 判断 Tooltip 面板是否会超出屏幕的右边缘或下边缘
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        float tooltipWidth = panel.rect.width;
        float tooltipHeight = panel.rect.height;

        // 确保面板不会超出屏幕边界
        if (pos.x + tooltipWidth + xOffset > screenWidth)
        {
            xOffset = -tooltipWidth - 10f; // 将面板移到左边
        }
        if (pos.y + tooltipHeight + yOffset > screenHeight)
        {
            yOffset = -tooltipHeight - 10f; // 将面板移到上方
        }

        // 应用偏移并更新面板位置
        panel.anchoredPosition = pos + new Vector2(xOffset, yOffset);
    }
}