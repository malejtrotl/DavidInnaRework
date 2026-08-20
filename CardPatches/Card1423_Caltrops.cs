using HarmonyLib;
using Rift;

namespace DavidInnaRework.CardPatches;

[HarmonyPatch(typeof(CardEffect), nameof(CardEffect.GetEffectCount))]
public static class CaltropsHitCountPatch
{
    private const int CaltropsCardId = 1423;
    private const int HitCount = 3;
    private const int UpgradedHitCount = 4;

    static void Prefix(CardEffect __instance)
    {
        var cardData = __instance.CardData;
        if (cardData == null || cardData._CardID != CaltropsCardId) return;
        if (__instance._Mode != EffectMode.Damage) return;

        __instance._EffectCount = HitCount;
        __instance._EffectCountUpgraded = UpgradedHitCount;
    }
}
