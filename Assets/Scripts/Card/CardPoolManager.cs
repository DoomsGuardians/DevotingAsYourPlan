using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CardPoolManager : MonoBehaviour
{
    public List<CardPool> predefinedPools;
    private Dictionary<string, CardPool> poolLookup = new();

    public void Initialize()
    {
        poolLookup.Clear();
        foreach (var pool in predefinedPools)
        {
            poolLookup[pool.poolName] = pool;
        }
    }

    public CardPool GetPool(string name) =>
        poolLookup.TryGetValue(name, out var pool) ? pool : null;

    public List<CardPool> GetPoolsWithTag(string tag) =>
        predefinedPools.Where(p => p.tags.Contains(tag)).ToList();
}
