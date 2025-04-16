using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Roles/Role Data")]
public class RoleData : ScriptableObject
{
    public RoleType type;
    public List<RoleStatInit> initialStats;
}

[System.Serializable]
public class RoleStatInit
{
    public string key;
    public float value;
}
