using System.Collections.Generic;

public class CardQueryBuilder
{
    private readonly CardQuery query = new();

    public static CardQueryBuilder New() => new();

    public CardQueryBuilder NameContains(string name)
    {
        query.nameContains = name;
        return this;
    }

    public CardQueryBuilder WithEntry(string entry)
    {
        query.requiredEntries ??= new();
        query.requiredEntries.Add(entry);
        return this;
    }

    public CardQueryBuilder WithEntries(params string[] entries)
    {
        query.requiredEntries ??= new();
        query.requiredEntries.AddRange(entries);
        return this;
    }

    public CardQueryBuilder RequireAllEntries(bool requireAll = true)
    {
        query.matchAllEntries = requireAll;
        return this;
    }

    public CardQueryBuilder WithType(CardType type)
    {
        query.typeFilter = type;
        return this;
    }

    public CardQueryBuilder MinRarity(int min)
    {
        query.rarityMin = min;
        return this;
    }

    public CardQueryBuilder MaxRarity(int max)
    {
        query.rarityMax = max;
        return this;
    }

    public CardQueryBuilder ExcludeUniques(bool exclude = true)
    {
        query.excludeUniques = exclude;
        return this;
    }

    public CardQueryBuilder Random(bool weighted = false)
    {
        query.randomPick = true;
        query.weightedByRarity = weighted;
        return this;
    }

    public CardQuery Build() => query;
}
