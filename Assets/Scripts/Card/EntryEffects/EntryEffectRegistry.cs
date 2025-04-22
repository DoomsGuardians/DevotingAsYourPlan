using System.Collections.Generic;

public static class EntryEffectRegistry
{
    private static readonly Dictionary<string, ICardEntryEffect> entryEffects = new()
    {
        { "Debug", new EntryEffect_Debug() },
        // 更多词条绑定…
    };

    public static bool TryGet(string entryName, out ICardEntryEffect effect) =>
        entryEffects.TryGetValue(entryName, out effect);
}
