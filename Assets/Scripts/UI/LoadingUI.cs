using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Cysharp.Threading.Tasks;

public class LoadingUI : MonoSingleton<LoadingUI>
{
    [SerializeField] private GameObject root;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TMP_Text progressText;

    private CanvasGroup canvasGroup;

    private void EnsureCanvasGroup()
    {
        if (canvasGroup == null)
        {
            canvasGroup = root.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = root.AddComponent<CanvasGroup>();
        }
    }

    public void Show()
    {
        EnsureCanvasGroup();
        root.SetActive(true);
        canvasGroup.alpha = 1f;
        SetProgress(0);
    }

    public void ShowWithText(string message)
    {
        Show();
        if (progressText != null)
            progressText.text = message;
    }

    public void SetProgress(float value)
    {
        if (progressBar != null)
            progressBar.value = value;

        if (progressText != null)
            progressText.text = $"{Mathf.RoundToInt(value * 100)}%";
    }

    public async UniTask FadeOutAndHide(float duration = 0.5f)
    {
        EnsureCanvasGroup();
        canvasGroup.DOFade(0f, duration);
        await UniTask.Delay((int)(duration * 1000));
        root.SetActive(false);
    }

    public override bool IsNotDestroyOnLoad() => true;
}