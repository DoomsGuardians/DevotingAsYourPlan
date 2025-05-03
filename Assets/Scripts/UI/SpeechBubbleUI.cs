using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using DG.Tweening;

public class SpeechBubbleUI : MonoBehaviour
{
    [Header("组件绑定")]
    public TMP_Text text;                 // 推荐使用 TMP_Text 更通用
    public CanvasGroup canvasGroup;

    [Header("动画设置")]
    public float showDuration = 0.3f;
    public float hideDuration = 0.4f;

    /// <summary>
    /// 展示气泡文本（自动淡入→保持→淡出→销毁）
    /// </summary>
    /// <param name="message">文本内容</param>
    /// <param name="totalDuration">整个生命周期时长</param>
    public async UniTask Show(string message, float totalDuration = 5f)
    {
        text.text = message;

        float holdTime = Mathf.Max(0.5f, totalDuration - showDuration - hideDuration);

        // 初始状态
        canvasGroup.alpha = 0f;
        transform.localScale = Vector3.zero;

        // 淡入 + 缩放入场
        canvasGroup.DOFade(1f, showDuration);
        await transform.DOScale(Vector3.one, showDuration).SetEase(Ease.OutBack).AsyncWaitForCompletion();

        // 保持停留
        await UniTask.Delay(System.TimeSpan.FromSeconds(holdTime));

        // 淡出 + 缩放消失
        canvasGroup.DOFade(0f, hideDuration);
        await transform.DOScale(Vector3.zero, hideDuration).SetEase(Ease.InBack).AsyncWaitForCompletion();

        Destroy(gameObject);
    }
}