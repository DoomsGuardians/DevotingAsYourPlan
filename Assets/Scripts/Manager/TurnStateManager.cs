using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class TurnStateManager
{
    private GameManager gameManager;
    private Dictionary<TurnPhase, TurnState> states;
    private TurnState currentState;
    public int TurnNum { get; set; } = 0;

    public TurnStateManager(GameManager manager)
    {
        gameManager = manager;
        states = new Dictionary<TurnPhase, TurnState>
        {
            { TurnPhase.StartTurn, new StartTurnState(manager) },
            { TurnPhase.DrawCard, new DrawCardState(manager) },
            { TurnPhase.PlayerAction, new PlayerActionState(manager) },
            { TurnPhase.NPCAction, new NPCActionState(manager) },
            { TurnPhase.ResolveEvents, new ResolveEventsState(manager) },
            { TurnPhase.EndTurn, new EndTurnState(manager) }
        };
    }


    public async UniTask TransitionToStateAsync(TurnPhase nextPhase)
    {
        currentState?.Exit();
        currentState = states[nextPhase];
        TurnPhaseEventSystem.RaisePhaseChanged(nextPhase);
        await currentState.EnterAsync();
    }

    public void Update()
    {
        currentState?.Update();
    }

    public bool IsCurrentPhase(TurnPhase phase)
    {
        return currentState == states[phase];
    }
}
