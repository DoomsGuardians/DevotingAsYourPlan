using UnityEngine;
using Naninovel;
using System.Collections;
using System.Collections.Generic;
using Naninovel.Commands;
using Naninovel.UI;

[CommandAlias("ExitDialogMode")]
public class SwitchToAdventureMode : Command
{
    public override async UniTask Execute(AsyncToken asyncToken)
    {
        await GameManager.Instance.ExitScenarioAsync();
    }
}