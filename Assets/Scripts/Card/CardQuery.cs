using System.Collections.Generic;
using UnityEngine;

public class CardQuery
{
    public string nameContains;
    public List<string> requiredEntries;
    public bool matchAllEntries = true;

    public CardType? typeFilter;
    public int? rarityMin;
    public int? rarityMax;

    public bool excludeUniques = false;

    public bool randomPick = false;
    public bool weightedByRarity = false;
}
