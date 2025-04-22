using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardType { Labor, Tribute, Believer }

[CreateAssetMenu(fileName = "CardData", menuName = "Cards/New Card")]
public class CardData : ScriptableObject
{
    public string cardName;
    [TextArea] public string description;
    public Sprite illustration;
    public CardType cardType;
    [Range(0, 3)] public int rarity;
    [Range(0, 100)]public int maxLife = 1;

    public bool isUnique = false;

    [Header("词条系统")]
    public List<CardEntry> entries;
}
