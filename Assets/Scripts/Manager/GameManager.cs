using UnityEngine;
using System.Collections.Generic;
using JetBrains.Annotations;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;

public class GameManager : MonoSingleton<GameManager>
{
    [Header("配置资源")]
    [SerializeField] private CardPool defaultCardPool;
    [SerializeField] public HorizontalCardHolder playerCardHolder;
    [SerializeField] private GameObject eventSlotPrefab;
    [SerializeField] private GameObject actionSlotPrefab;
    [SerializeField] private List<RoleData> roleDataConfigs;
    [SerializeField] private RoleStatDefinitionTable statDefinitionTable;
    [SerializeField] private EventNodeDataPool defaultEventNodeDatas;
    public List<RectTransform> eventHolders;
    
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
        EventManager.Initialize(defaultEventNodeDatas.eventNodeDataList, eventSlotPrefab, actionSlotPrefab, eventHolders);
        Debug.Log("GameManager初始化完成");
        turnStateMachine = new TurnStateManager(this);
    }

    private async void Start()
    {
        Debug.Log("开始游戏启动流程");
        
        // 随机播放背景音乐
        AudioManager.Instance.PlayRandomBGM("bgm_reverberation_protocol", "bgm_you_mean_it", "bgm_confession_beneath_the_smile");
        await TransitionToStateAsync(TurnPhase.StartTurn);
    }

    private void Update()
    {
        turnStateMachine.Update();
    }

    public async UniTask TransitionToStateAsync(TurnPhase phase)
    {
        await turnStateMachine.TransitionToStateAsync(phase);
    }


    public async UniTask DrawCardsAsync()//此后将用于处理每回合开始人民奉献给玩家的卡牌
    {
        
        await CardManager.DrawCardsAsync();
        
    }

    public Role GetRole(RoleType type) => RoleManager.GetRole(type);

    public async UniTask ProcessEventTrigger() => await EventManager.ProcessEventTrigger();

    public async UniTask ProcessPlayerDefaultTrigger() => await EventManager.ProcessPlayerDefault();

    public async UniTask ResolveEventEffect() => await EventManager.ResolveEventsEffectAsync();

    public void TickCoolDown() => EventManager.TickCooldowns();

    public void TickEvents() => EventManager.TickEvents();

    public void TickCards() => CardManager.TickCards();

    public void RefreshCards() => CardManager.RefreshCards();

    public void SettleAllRoles() => RoleManager.SettleAllRoles();
    
    public override bool IsNotDestroyOnLoad() => false;
}