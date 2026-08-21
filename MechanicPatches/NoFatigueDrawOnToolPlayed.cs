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
}

// Ensures the target card persistently carries both the marker flag and
// NoFatigueSpecialDraw, regardless of how/when the game touches it.
[HarmonyPatch(typeof(CardData), nameof(CardData.GetDescription))]
public static class NoFatigueDrawOnToolPlayedModifierPatch
{
    static void Prefix(CardData __instance)
    {
        if (__instance == null || __instance._CardID != NoFatigueDrawOnToolPlayedState.TargetCardId) return;

        __instance._Modifiers |= CardModifiers.NoFatigueSpecialDraw | NoFatigueDrawOnToolPlayedState.DrawMarker;
    }
}

[HarmonyPatch(typeof(CombatManager), nameof(CombatManager.UseCard))]
public static class NoFatigueDrawOnToolPlayedPatch
{
    static void Prefix(Card card, Entity castingEntity)
    {
        if (card == null || castingEntity == null) return;
        if (NoFatigueDrawOnToolPlayedState.TargetCardId < 0) return;

        var cardData = card.Data;
        if (cardData == null || cardData._CardType != CardType.Tool) return;

        // Ensure the target card carries both flags even if GetDescription has
        // not fired yet for this CardData instance.
        var targetCard = FindCard(castingEntity._Deck) ?? FindCard(castingEntity._DiscardPile);
        if (targetCard?.Data != null)
        {
            targetCard.Data._Modifiers |= CardModifiers.NoFatigueSpecialDraw | NoFatigueDrawOnToolPlayedState.DrawMarker;
        }

        castingEntity.DrawCardsWithModifier(NoFatigueDrawOnToolPlayedState.DrawMarker);
    }

    private static Card FindCard(Il2CppSystem.Collections.Generic.List<Card> pile)
    {
        if (pile == null) return null;

        foreach (var candidate in pile)
        {
            if (candidate?.Data != null && candidate.Data._CardID == NoFatigueDrawOnToolPlayedState.TargetCardId)
            {
                return candidate;
            }
        }

        return null;
    }
}
