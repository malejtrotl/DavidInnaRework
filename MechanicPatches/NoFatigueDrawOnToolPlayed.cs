using HarmonyLib;
using Rift;

namespace DavidInnaRework.MechanicPatches;

// Generic trigger for: "When a Tool is played, [target card] draws itself
// without counting toward fatigue."
//
// There is no native "on Tool played" trigger modifier, so this reuses the
// unused bit left in the CardModifiers [Flags] enum (bit 0, value 1) as a
// private marker.
public static class NoFatigueDrawOnToolPlayedState
{
    // Card ID to draw whenever a Tool is played. Set to -1 to disable.
    internal static int TargetCardId = -1;

    // Private marker bit standing in for a "DrawOnToolPlayed" trigger flag.
    internal const CardModifiers DrawMarker = (CardModifiers)1;

    public static void Configure(int cardId)
    {
        TargetCardId = cardId;
    }

    // Static, purely data/flag mutation (no text involved) — marks the target
    // card so it can always be found by DrawCardsWithModifier and never
    // counts toward fatigue. Applied once via
    // MechanicPatches/CardDataGameLoadInitializer.cs at real game-load time.
    public static void ApplyMutations(CardData cardData)
    {
        if (cardData == null || cardData._CardID != TargetCardId) return;

        cardData._Modifiers |= CardModifiers.NoFatigueSpecialDraw | DrawMarker;
    }
}

// The actual trigger is inherently event-driven (fires live, each time a Tool
// is played during a match), so unlike the flag setup above, this stays a
// per-call Harmony patch.
[HarmonyPatch(typeof(CombatManager), nameof(CombatManager.UseCard))]
public static class NoFatigueDrawOnToolPlayedPatch
{
    static void Prefix(Card card, Entity castingEntity)
    {
        if (card == null || castingEntity == null) return;
        if (NoFatigueDrawOnToolPlayedState.TargetCardId < 0) return;

        var cardData = card.Data;
        if (cardData == null || cardData._CardType != CardType.Tool) return;

        castingEntity.DrawCardsWithModifier(NoFatigueDrawOnToolPlayedState.DrawMarker);
    }
}
