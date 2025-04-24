using UnityEngine;
using Naninovel;
using System.Collections;
using System.Collections.Generic;
using Naninovel.Commands;
using Naninovel.UI;

[CommandAlias("ExitDialogMode")]
public class SwitchToAdventureMode : Command
{
    public override async UniTask Execute (AsyncToken asyncToken)
    {
        // // 1. Disable Naninovel input.
        // var inputManager = Engine.GetService<IInputManager>();
        // inputManager.ProcessInput = false;

        // 2. Stop script player.
        var scriptPlayer = Engine.GetService<IScriptPlayer>();
        scriptPlayer.Stop();
        
        var stateManager = Engine.GetService<IStateManager>();
        // 播放完后，你可以在这里恢复游戏逻辑
        await stateManager.ResetState();
        
    }
}