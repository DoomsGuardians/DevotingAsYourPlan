using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoSingleton<GameManager>
{
    [Header("配置资源")]
    [SerializeField]private List<CardData> cardDatabase; 
    [SerializeField]private HorizontalCardHolder playerCardHolder;
    [SerializeField] private GameObject eventSlotPrefab;
    [SerializeField] private List<RoleData> roleDataConfigs;
    [SerializeField] private RoleStatDefinitionTable statDefinitionTable;
    [SerializeField] private List<EventNodeData> defaultEventNodeDatas;
    
    #region 回合制状态机
        public TurnStateMachine turnStateMachine;
    #endregion

    #region 管理器
    public CardManager CardManager { get; private set; }
    public RoleManager RoleManager { get; private set; }
    public EventManager EventManager { get; private set; }
    #endregion


    protected override void Awake()
    {
        base.Awake(); // 确保 MonoSingleton 的生命周期处理被调用

        turnStateMachine = new TurnStateMachine();
        CardManager = new CardManager();
        RoleManager = new RoleManager();
        EventManager = new EventManager();
        
        CardManager.Initialize(cardDatabase, playerCardHolder);
        RoleManager.Initialize(roleDataConfigs, statDefinitionTable);
        EventManager.Initialize(defaultEventNodeDatas);
        EventSlotFactory.Initialize(eventSlotPrefab);
        turnStateMachine.Initialize(this);
        
        Debug.Log("GameManager初始化完成");
    }

    private void Update()
    {
        turnStateMachine.Update();
    }

    public void TransitionToState(TurnPhase phase)
    {
        turnStateMachine.TransitionToState(phase);
    }

    public void DrawCards()//此后将用于处理每回合开始人民奉献给玩家的卡牌
    {
        
        CardManager.DrawCard(CardType.Labor);
        //CardManager.DrawCard(CardType.Believer);
        CardManager.DrawCard(CardType.Tribute);
        
    }

    public Role GetRole(RoleType type) => RoleManager.GetRole(type);
    
    public override bool IsNotDestroyOnLoad() => true;
}