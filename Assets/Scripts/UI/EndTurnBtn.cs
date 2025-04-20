using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndTurnBtn : MonoBehaviour
{
    public void EndPlayerTurn()
    {
        GameManager.Instance.TransitionToState(TurnPhase.ResolveEvents);
    }
}
