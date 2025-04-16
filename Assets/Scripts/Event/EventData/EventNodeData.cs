using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/Event Node")]
public class EventNodeData : ScriptableObject
{
    public string eventID;
    public string eventName;
    public RoleType sourceRole;

    public List<EventConditionSO> triggerConditions;
    public List<EventOutcomeBranch> outcomeBranches;

    [TextArea]
    public string description;
    public Sprite icon;
    public int duration = 1;
}
