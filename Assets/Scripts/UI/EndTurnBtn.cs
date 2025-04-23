
using UnityEngine;
using Cysharp.Threading.Tasks;

public class EndTurnBtn : MonoBehaviour
{
    public async UniTask EndPlayerTurn()
    {
        Debug.Log("玩家点击了结束回合");
        await GameManager.Instance.TransitionToStateAsync(TurnPhase.ResolveEvents);
    }
}

