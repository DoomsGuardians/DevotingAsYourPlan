using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CrowdSpeechConfig", menuName = "Crowd Speech/Config")]
public class CrowdSpeechConfigSO : ScriptableObject
{
    public List<CrowdSpeechRule> rules = new();
}

[System.Serializable]
public class CrowdSpeechRule
{
    public RoleType sourceRole = RoleType.World; 
    [RoleStatKey]
    public string statKey;
    public FloatRange range;
    public List<string> speeches = new();

    [HideInInspector] public Queue<string> recentQueue = new();
}


[System.Serializable]
public struct FloatRange
{
    public float min, max;
    public bool InRange(float val) => val >= min && val <= max;
}