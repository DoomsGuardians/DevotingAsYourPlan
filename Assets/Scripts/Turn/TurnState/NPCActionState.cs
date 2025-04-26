using UnityEngine;
using Cysharp.Threading.Tasks;
public class NPCActionState : TurnState
{
    public NPCActionState(GameManager manager) : base(manager) { }

    public override async UniTask EnterAsync()
    {
        Debug.Log("进入角色行动阶段");
        AudioManager.Instance.PlaySFX("turn_transition");
        if (gameManager.turnTransitionText.TurnStrings[8] != "" && gameManager.turnTransitionText.TurnStrings[9] != "")
        {
            AudioManager.Instance.PlaySFX("turn_transition");
            await TurnStateHandler.Instance.TurnStateAnim(gameManager.turnTransitionText.TurnStrings[8], gameManager.turnTransitionText.TurnStrings[9]);
        }
        await gameManager.ProcessEventTrigger();
        await UniTask.Yield(); // 保留异步格式
    }
}