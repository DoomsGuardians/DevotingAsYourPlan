using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnStateMachine
{
    private Dictionary<TurnPhase, TurnState> states = new();
    private TurnState currentState;

    private int turnNum = 0;

    public int TurnNum
    {
        get => turnNum;
        set => turnNum = value;
    }

    public void Initialize(GameManager gameManager)
    {
        states[TurnPhase.StartTurn] = new StartTurnState(gameManager);
        states[TurnPhase.DrawCard] = new DrawCardState(gameManager);
        states[TurnPhase.PlayerAction] = new PlayerActionState(gameManager);
        states[TurnPhase.NPCAction] = new NPCActionState(gameManager);
        states[TurnPhase.ResolveEvents] = new ResolveEventsState(gameManager);
        states[TurnPhase.EndTurn] = new EndTurnState(gameManager);

        TransitionToState(TurnPhase.StartTurn);
    }

    public void TransitionToState(TurnPhase phase)
    {
        currentState?.Exit();
        currentState = states[phase];
        currentState.Enter();
    }

    public void Update()
    {
        currentState?.Update();
    }
}