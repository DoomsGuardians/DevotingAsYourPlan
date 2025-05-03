using UnityEngine;
using System.Collections.Generic;
using TMPro;

[CreateAssetMenu(menuName = "UI/Stat Overview Config")]
public class StatOverviewConfigSO : ScriptableObject
{
    public List<StatOverviewEntry> entries;
}

[System.Serializable]
public class StatOverviewEntry
{
    public RoleType sourceRole;
    [RoleStatKey] public string statKey;
    public TMP_Text uiText;
}