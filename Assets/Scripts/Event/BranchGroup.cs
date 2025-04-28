using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "Events/分支组")]
public class BranchGroup : ScriptableObject
{
    public string label = "";

    [Tooltip("A list of EventOutcomeBranches")]
    public List<EventOutcomeBranch> branches; // List of EventOutcomeBranch
}