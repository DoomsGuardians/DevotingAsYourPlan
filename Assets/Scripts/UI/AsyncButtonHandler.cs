using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System;
using System.Reflection;

[RequireComponent(typeof(Button))]
public class AsyncButtonHandler : MonoBehaviour
{
    [Header("绑定逻辑方法")]
    public MonoBehaviour targetScript;

    [Tooltip("方法名称（需要返回 UniTask）")]
    public string methodName;

    [Header("控制选项")]
    [Tooltip("点击按钮后自动禁用防止多次点击")]
    public bool disableButtonDuringTask = true;

    [Header("回合阶段限制（可选）")]
    [Tooltip("若启用，仅在指定回合阶段可点击")]
    public bool restrictToPhase = false;
    public TurnPhase allowedPhase = TurnPhase.PlayerAction;

    [SerializeField] private Button button;
    private MethodInfo targetMethod;
    private bool isBusy = false;

    private void Awake()
    {
        // 确保 targetScript 和 methodName 被正确配置
        if (targetScript == null || string.IsNullOrEmpty(methodName))
        {
            Debug.LogWarning($"[AsyncButtonHandler] `{name}` 未配置目标脚本或方法名");
            return;
        }

        // 通过反射获取方法
        targetMethod = targetScript.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        // 确保方法存在且返回类型是 UniTask
        if (targetMethod == null || targetMethod.ReturnType != typeof(UniTask))
        {
            Debug.LogError($"[AsyncButtonHandler] `{methodName}` 未找到，或返回类型不是 UniTask");
            return;
        }

        // 绑定按钮点击事件
        button.onClick.AddListener(OnClick);

        // 如果启用回合阶段限制，订阅阶段变化事件
        if (restrictToPhase)
        {
            TurnPhaseEventSystem.OnPhaseChanged += HandlePhaseChanged;
            // 初始化阶段状态
            HandlePhaseChanged();
        }
    }

    private void OnDestroy()
    {
        // 取消订阅阶段变化事件
        if (restrictToPhase)
        {
            TurnPhaseEventSystem.OnPhaseChanged -= HandlePhaseChanged;
        }
    }

    private void HandlePhaseChanged()
    {
        if (!isBusy)
            button.interactable = GameManager.Instance.turnStateMachine.IsCurrentPhase(allowedPhase);
    }

    private void OnClick()
    {
        // 防止多次点击
        if (isBusy) return;

        // 如果启用回合阶段限制，检查当前阶段
        if (restrictToPhase && !GameManager.Instance.turnStateMachine.IsCurrentPhase(allowedPhase))
        {
            Debug.LogWarning($"当前阶段不是 `{allowedPhase}`，按钮 `{name}` 禁止点击");
            return;
        }

        isBusy = true;
        // 禁用按钮，防止重复点击
        if (disableButtonDuringTask)
            button.interactable = false;

        // 执行异步任务
        InvokeAsync().Forget();
    }

    private async UniTaskVoid InvokeAsync()
    {
        try
        {
            // 使用反射调用方法并检查返回类型
            var result = targetMethod.Invoke(targetScript, null);

            // 如果返回的是 UniTask 类型，等待它完成
            if (result is UniTask task)
            {
                await task;  // 等待 UniTask 完成
            }
            // 如果返回的是 UniTaskVoid 类型，直接执行（无需等待）
            else if (result is UniTaskVoid)
            {
                // UniTaskVoid 不需要 await，直接执行
                // 不做任何处理，任务会自动执行完毕
            }
            else
            {
                Debug.LogError("[AsyncButtonHandler] 返回的不是 UniTask 或 UniTaskVoid 类型");
            }
        }
        catch (OperationCanceledException)
        {
            // 处理任务被取消的情况
            Debug.LogWarning("[AsyncButtonHandler] 操作被取消");
        }
        catch (Exception ex)
        {
            // 捕获其他异常
            Debug.LogError($"[AsyncButtonHandler] 调用出错: {ex.Message}");
        }
        finally
        {
            // 任务完成后恢复按钮状态
            isBusy = false;
            if (restrictToPhase)
                HandlePhaseChanged();
            else if (disableButtonDuringTask)
                button.interactable = true;
        }
    }
}
