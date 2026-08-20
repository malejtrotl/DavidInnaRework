using HarmonyLib;
using Rift;

namespace DavidInnaRework.CardPatches;

// Card 1408 "Frantic Scouring" gains a NEW when-discarded effect that creates
// 2 Tools (3 upgraded). The card already creates Tools when played; that
// on-play effect is left untouched.
//
// Because the discard effect does not exist on the card yet, this adds a brand
// new CardEffect rather than mutating an existing one.
//
// Hooked on CardData.GetDescription because it fires once per card (unlike
// GetFinalValue, which fires once per effect). The guard over _Effects makes
// this idempotent, since the Prefix runs on every call — and it matches on the
// WhenDiscarded modifier so it won't confuse our effect with the card's
// existing on-play CreateTool effect.
//
// CardData = __instance is required: effects loaded from the game's assets
// already have this owner back-reference, but `new CardEffect` does not, and a
// missing owner causes a NullReferenceException at runtime.
[HarmonyPatch(typeof(CardData), nameof(CardData.GetDescription))]
public static class FranticScouringDiscardCreatesToolsPatch
{
    private const int FranticScouringCardId = 1408;
    private const int ToolsCreated = 2;
    private const int ToolsCreatedUpgraded = 3;

    static void Prefix(CardData __instance)
    {
        if (__instance == null || __instance._CardID != FranticScouringCardId) return;

        foreach (var existingEffect in __instance._Effects)
        {
            if (existingEffect._Mode == EffectMode.CreateTool
                && existingEffect._Modifiers == EffectModifiers.WhenDiscarded)
            {
                return;
            }
        }

        __instance._Effects.Add(new CardEffect
        {
            CardData = __instance,
            _Mode = EffectMode.CreateTool,
            _Modifiers = EffectModifiers.WhenDiscarded,
            _Targeting = EffectTargeting.Self,
            _EffectValue = ToolsCreated,
            _EffectValueUpgraded = ToolsCreatedUpgraded,
        });
    }
}

// Placeholder for Frantic Scouring's updated tooltip text, needed because the
// card now also creates Tools when discarded.
//
// PLACEHOLDER: replace NewDescription with the real wording (keeping {0}/{1}
// placeholders for the on-play and when-discarded tool counts so the game
// fills in the live/upgraded values itself).
[HarmonyPatch(typeof(CardData), nameof(CardData.GetDescription))]
public static class FranticScouringDescriptionPatch
{
    private const int FranticScouringCardId = 1408;
    private const string NewDescription = "Discard {0} to create {1} Tools.\nCreate {2} Tools when discarded.";

    static void Prefix(CardData __instance)
    {
        if (__instance == null || __instance._CardID != FranticScouringCardId) return;

        __instance._BaseDescription = NewDescription;
    }
}
