using System.Collections.Generic;
using UnityEngine;

public static class CardHolderManager
{
    public static readonly List<HorizontalCardHolder> Holders = new List<HorizontalCardHolder>();

    public static void Register(HorizontalCardHolder holder)
    {
        if (!Holders.Contains(holder))
            Holders.Add(holder);
    }

    public static void Unregister(HorizontalCardHolder holder)
    {
        if (Holders.Contains(holder))
            Holders.Remove(holder);
    }
}