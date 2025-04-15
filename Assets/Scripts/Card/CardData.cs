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
    public int maxLife = 1;

    // 可扩展属性（比如参数机制）
    //public List<CardTag> tags; // 可设计成 enum 或 string-based key

    // 后期可以加权重、事件链ID、特殊效果等
}
