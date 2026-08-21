using HarmonyLib;
using Rift;

namespace DavidInnaRework.CardPatches;

// Card 1426 "Unstable Darkstone" gains a NEW Dispel effect for 2 (3 upgraded)
// with Ranged targeting. Its existing effects are left untouched.
//
// Follows the Bottled Ectoplasm template: a Prefix on CardData.GetDescription
// (fires once per card, unlike GetFinalValue which fires once per effect),
// with a guard over _Effects making it idempotent since the Prefix runs on
// every call.
//
// CardData = __instance is required: effects loaded from the game's assets
// already have this owner back-reference, but `new CardEffect` does not, and a
// missing owner causes a NullReferenceException at runtime.
[HarmonyPatch(typeof(CardData), nameof(CardData.GetDescription))]
public static class UnstableDarkstoneDispelPatch
{
    private const int UnstableDarkstoneCardId = 1426;
    private const int DispelCount = 2;
    private const int DispelCountUpgraded = 3;

    static void Prefix(CardData __instance)
    {
        if (__instance == null || __instance._CardID != UnstableDarkstoneCardId) return;

        foreach (var existingEffect in __instance._Effects)
        {
            if (existingEffect._Mode == EffectMode.Dispel)
            {
                return;
            }
        }

        var dispelEffect = new CardEffect
        {
            CardData = __instance,
            _Mode = EffectMode.Dispel,
            _Targeting = EffectTargeting.Ranged,
            _EffectValue = DispelCount,
            _EffectValueUpgraded = DispelCountUpgraded,
        };

        __instance._Effects.Add(dispelEffect);
    }
}

// Placeholder for Unstable Darkstone's fully rewritten tooltip text, needed
// because of the added Dispel effect.
//
// PLACEHOLDER: replace NewDescription with the real wording (keeping
// {0}/{1}... placeholders for the numbers so the game fills in the
// live/upgraded values itself).
[HarmonyPatch(typeof(CardData), nameof(CardData.GetDescription))]
public static class UnstableDarkstoneDescriptionPatch
{
    private const int UnstableDarkstoneCardId = 1426;
    private const string NewDescription = "Give Doom ({0}) to any enemy and Dispel it {2} times. Reduce cost by {1} when you play a non-Mana card.";

    static void Prefix(CardData __instance)
    {
        if (__instance == null || __instance._CardID != UnstableDarkstoneCardId) return;

        __instance._BaseDescription = NewDescription;
    }
}
