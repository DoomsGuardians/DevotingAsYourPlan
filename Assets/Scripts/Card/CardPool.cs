using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CardPool", menuName = "Cards/Card Pool")]
public class CardPool : ScriptableObject
{
    public string poolName;
    [TextArea] public string description;

    public List<CardData> cards;
    public List<string> tags;
}
