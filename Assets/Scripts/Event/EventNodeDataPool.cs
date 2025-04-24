using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "EventNodeDataPool",menuName = "Events/事件池")]
public class EventNodeDataPool : ScriptableObject
{
    public string eventPoolName;
    public List<EventNodeData> eventNodeDataList;
}
