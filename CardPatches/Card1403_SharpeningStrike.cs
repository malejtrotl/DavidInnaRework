using HarmonyLib;
using Rift;

namespace DavidInnaRework.CardPatches;

// Card 1403 "Sharpening Strike" keeps all of its existing effects, and gains
// an additional IncreaseDamage effect for 2 (3 upgraded).
//
// Hooked on CardData.GetDescription because it fires once per card (unlike
// GetFinalValue, which fires once per effect). The guard over _Effects makes
// this idempotent, since the Prefix runs on every call.
//
// CardData = __instance is required: effects loaded from the game's assets
// already have this owner back-reference, but `new CardEffect` does not, and a
// missing owner causes a NullReferenceException at runtime.
[HarmonyPatch(typeof(CardData), nameof(CardData.GetDescription))]
public static class SharpeningStrikeIncreaseDamagePatch
{
    private const int SharpeningStrikeCardId = 1403;
    private const int DamageIncrease = 2;
    private const int DamageIncreaseUpgraded = 3;

    static void Prefix(CardData __instance)
    {
        if (__instance == null || __instance._CardID != SharpeningStrikeCardId) return;

        foreach (var existingEffect in __instance._Effects)
        {
            if (existingEffect._Mode == EffectMode.IncreaseDamage)
            {
                return;
            }
        }

        __instance._Effects.Add(new CardEffect
        {
            CardData = __instance,
            _Mode = EffectMode.IncreaseDamage,
            _Targeting = EffectTargeting.Self,
            _EffectValue = DamageIncrease,
            _EffectValueUpgraded = DamageIncreaseUpgraded,
        });
    }
}

[HarmonyPatch(typeof(CardData), nameof(CardData.GetDescription))]
public static class SharpeningStrikeDescriptionPatch
{
    private const int SharpeningStrikeCardId = 1403;
    private const string NewDescription = "Deal {0} damage to the first enemy. Increase the damage of this and all Strikes in your hand by {1}.";

    static void Prefix(CardData __instance)
    {
        if (__instance == null || __instance._CardID != SharpeningStrikeCardId) return;

        __instance._BaseDescription = NewDescription;
    }
}
