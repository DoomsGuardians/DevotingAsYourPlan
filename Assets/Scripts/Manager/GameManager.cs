using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoSingleton<GameManager>
{
    [SerializeField]private List<CardData> cardDatabase; 
    [SerializeField]private HorizontalCardHolder playerCardHolder;
    
    public TurnStateMachine turnStateMachine;

    public CardManager CardManager { get; private set; }
    public EventManager EventManager { get; private set; }

    protected override void Awake()
    {
        base.Awake(); // 确保 MonoSingleton 的生命周期处理被调用

        turnStateMachine = new TurnStateMachine();
        CardManager = new CardManager();
        EventManager = new EventManager();
        CardManager.Initialize(cardDatabase, playerCardHolder);
        turnStateMachine.Initialize(this);
        
        Debug.Log("初始化完成");
    }

    private void Update()
    {
        turnStateMachine.Update();
    }

    public void TransitionToState(TurnPhase phase)
    {
        turnStateMachine.TransitionToState(phase);
    }

    public void DrawCards()
    {
        
        CardManager.DrawCard(CardType.Labor);
        //CardManager.DrawCard(CardType.Believer);
        CardManager.DrawCard(CardType.Tribute);
        
    }

    public override bool IsNotDestroyOnLoad() => true;
}