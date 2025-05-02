using UnityEngine;
using System.Collections.Generic;
using JetBrains.Annotations;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using Naninovel;
using System.Linq;
using UniTask = Naninovel.UniTask;

public class GameManager : MonoSingleton<GameManager>
{
    [Header("配置资源")] [SerializeField] private CardPool defaultCardPool; //默认牌库
    [SerializeField] public HorizontalCardHolder playerCardHolder;
    [SerializeField] private GameObject eventSlotPrefab;
    [SerializeField] private GameObject actionSlotPrefab;
    [SerializeField] private List<RoleData> roleDataConfigs;
    [SerializeField] private RoleStatDefinitionTable statDefinitionTable;
    [SerializeField] private EventNodeDataPool defaultEventNodeDatas;
    [SerializeField] public TurnTransitionTextSO turnTransitionText;
    [SerializeField] private CardPool initialCardPool; // 玩家初始套牌
    [SerializeField] private List<CardEntry> runtimeEntries;
    public List<RectTransform> eventHolders;
    [SerializeField] private string IntroScenario;

    #region NaniNovel相关

    public async Cysharp.Threading.Tasks.UniTask PlayScenarioAsync(string scriptName)
    {
        foreach (var holder in eventHolders)
        {
            holder.GetComponent<CanvasGroup>().blocksRaycasts = false;
        }

        playerCardHolder.GetComponent<CanvasGroup>().blocksRaycasts = false;

        if (!Engine.Initialized) await RuntimeInitializer.Initialize();
        // 1.Enable Naninovel input.
        var inputManager = Engine.GetService<IInputManager>();
        inputManager.ProcessInput = true;

        if (string.IsNullOrEmpty(scriptName))
        {
            Debug.LogWarning("[NaninovelEffect] 未设置剧本名");
            return;
        }

        var player = Engine.GetService<IScriptPlayer>();
        if (player == null)
        {
            Debug.LogError("[NaninovelEffect] 无法获取 IScriptPlayer");
            return;
        }

        try
        {
            Debug.Log($"[NaninovelEffect] 播放剧本：{scriptName}");

            // Play the script
            await player.LoadAndPlay(scriptName);

            // Wait for the script to finish using the event system
            await WaitForScriptToFinish(player);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[NaninovelEffect] 播放剧本时出错：{ex.Message}");
        }
    }

    public async Cysharp.Threading.Tasks.UniTask ExitScenarioAsync()
    {
        foreach (var holder in eventHolders)
        {
            holder.GetComponent<CanvasGroup>().interactable = true;
            holder.GetComponent<CanvasGroup>().blocksRaycasts = true;
        }

        playerCardHolder.GetComponent<CanvasGroup>().interactable = true;
        playerCardHolder.GetComponent<CanvasGroup>().blocksRaycasts = true;

        // 1. Disable Naninovel input.
        var inputManager = Engine.GetService<IInputManager>();
        inputManager.ProcessInput = false;

        // 2. Stop script player.
        var scriptPlayer = Engine.GetService<IScriptPlayer>();
        scriptPlayer.Stop();

        var stateManager = Engine.GetService<IStateManager>();
        // 播放完后，你可以在这里恢复游戏逻辑
        await stateManager.ResetState();
    }


    // Helper method to wait for script to finish
    private async Cysharp.Threading.Tasks.UniTask WaitForScriptToFinish(IScriptPlayer player)
    {
        // This callback will be called when the script finishes
        var completionTask = new Cysharp.Threading.Tasks.UniTaskCompletionSource();
        var script = player.PlayedScript;
        player.OnStop += (script) => completionTask.TrySetResult(); // Subscribe to script finished event

        await completionTask.Task; // Wait until the script finishes

        // Unsubscribe from the events once done
        player.OnStop -= (script) => completionTask.TrySetResult();
    }

    #endregion


    #region 回合制状态机

    public TurnStateManager turnStateMachine;

    #endregion

    #region 管理器

    public CardManager CardManager { get; private set; }
    public RoleManager RoleManager { get; private set; }
    public EventManager EventManager { get; private set; }

    #endregion


    protected override void Awake()
    {
        base.Awake(); // 确保 MonoSingleton 的生命周期处理被调用

        CardManager = new CardManager();
        RoleManager = new RoleManager();
        EventManager = new EventManager();

        CardManager.Initialize(defaultCardPool, playerCardHolder);
        RoleManager.Initialize(roleDataConfigs, statDefinitionTable);
        EventManager.Initialize(defaultEventNodeDatas.eventNodeDataList, eventSlotPrefab, actionSlotPrefab,
            eventHolders);
        Debug.Log("GameManager初始化完成");
        turnStateMachine = new TurnStateManager(this);
    }

    private async void Start()
    {
        Debug.Log("开始游戏启动流程");

        if (IntroScenario != "")
            await PlayScenarioAsync(IntroScenario);
        // 随机播放背景音乐
        AudioManager.Instance.PlayRandomBGM(AudioManager.Instance.audioConfig.GetKeysByPrefix("bgm_").ToArray());
        CardManager.LoadInitialCards(playerCardHolder, initialCardPool);
        await TransitionToStateAsync(TurnPhase.StartTurn);
    }

    private void Update()
    {
        turnStateMachine.Update();
    }

    public async Cysharp.Threading.Tasks.UniTask TransitionToStateAsync(TurnPhase phase)
    {
        await turnStateMachine.TransitionToStateAsync(phase);
    }

    public CardEntry GetEntry(string specificName)
    {
        var entry = runtimeEntries.FirstOrDefault(e => e.entryName == specificName);
        if (entry == null)
        {
            Debug.LogWarning($"未能找到名为 {specificName} 的条目！");
            return null; // 返回 null 或者其他错误处理
        }

        return entry;
    }

    public async Cysharp.Threading.Tasks.UniTask DrawCardsAsync() //此后将用于处理每回合开始人民奉献给玩家的卡牌
    {
        CardManager.GiveConformityOrthodoxyEntries(playerCardHolder.cards);
        await CardManager.DrawCardsAsync();
    }

    public Role GetRole(RoleType type) => RoleManager.GetRole(type);

    public async Cysharp.Threading.Tasks.UniTask ProcessEventTrigger() => await EventManager.ProcessEventTrigger();

    public async Cysharp.Threading.Tasks.UniTask ProcessPlayerDefaultTrigger() =>
        await EventManager.ProcessPlayerDefault();

    public async Cysharp.Threading.Tasks.UniTask ResolveEventEffect() => await EventManager.ResolveEventsEffectAsync();

    public void TickCoolDown() => EventManager.TickCooldowns();

    public void TickEvents() => EventManager.TickEvents();

    public void TickCards() => CardManager.TickCards();

    public void RefreshCards() => CardManager.RefreshCards();

    public void SettleAllRoles() => RoleManager.SettleAllRoles();

    public async Cysharp.Threading.Tasks.UniTask CheckForEnding()
    {
        var player = RoleManager.GetRole(RoleType.Player);
        if (player.GetStat("健康度") <= 0)
        {
            var endingText = new EndingManager().GenerateEndingSummary();
            Debug.Log("结局生成：\n" + endingText);
            foreach (var holder in eventHolders)
            {
                holder.GetComponent<CanvasGroup>().blocksRaycasts = false;
            }

            playerCardHolder.GetComponent<CanvasGroup>().blocksRaycasts = false;
            // 可在此调用 UI 展示 或 播放 Naninovel 剧本
            await EndingPanel.Instance.Show(endingText);
            AudioManager.Instance.PlaySFX("turn_transition");
            await InputUtility.WaitForClickAsync();
            await EndingPanel.Instance.Hide();
            await RestartGameSession();
        }
        
    }
    
    private void ClearSceneObjects()
    {
        // 清空所有事件 GameObject（建议用 Tag 或统一挂载节点管理）
        foreach (var holder in eventHolders)
        {
            foreach (Transform child in holder)
            {
                GameObject.Destroy(child.gameObject);
            }
        }

        // 清空所有卡牌
        foreach (var card in playerCardHolder.cards.ToList())
        {
            playerCardHolder.DestroyCard(card);
        }
    }
    
    public async UniTask RestartGameSession()
    {
        LoadingUI.Instance.ShowWithText("重新开始中...");

        ClearSceneObjects(); // 主动清除所有事件与卡牌对象

        // 重新初始化逻辑组件
        CardManager = new CardManager();
        RoleManager = new RoleManager();
        EventManager = new EventManager();

        CardManager.Initialize(defaultCardPool, playerCardHolder);
        RoleManager.Initialize(roleDataConfigs, statDefinitionTable);
        EventManager.Initialize(defaultEventNodeDatas.eventNodeDataList, eventSlotPrefab, actionSlotPrefab, eventHolders);

        turnStateMachine = new TurnStateManager(this);
        
        await LoadingUI.Instance.FadeOutAndHide();
        
        CardManager.LoadInitialCards(playerCardHolder, initialCardPool);
        await TransitionToStateAsync(TurnPhase.StartTurn);

        
    }

    
    public override bool IsNotDestroyOnLoad() => false;
}