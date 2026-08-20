using HarmonyLib;
using Rift;

namespace DavidInnaRework.CardPatches;

[HarmonyPatch(typeof(CardEffect), nameof(CardEffect.GetEffectCount))]
public static class CleansingBalmCleanseCountPatch
{
    private const int CleansingBalmCardId = 1418;
    private const int CleanseCount = 2;
    private const int UpgradedCleanseCount = 3;

    static void Prefix(CardEffect __instance)
    {
        var cardData = __instance.CardData;
        if (cardData == null || cardData._CardID != CleansingBalmCardId) return;
        if (__instance._Mode != EffectMode.Cleanse) return;

        __instance._EffectValue = CleanseCount;
        __instance._EffectValueUpgraded = UpgradedCleanseCount;
    }
}

[HarmonyPatch(typeof(CardData), nameof(CardData.GetDescription))]
public static class CleansingBalmDescriptionPatch
{
    private const int CleansingBalmCardId = 1418;

    static void Prefix(CardData __instance)
    {
        if (__instance == null || __instance._CardID != CleansingBalmCardId) return;

        __instance._BaseDescription = __instance._BaseDescription
            .Replace(" time.", " times.");
    }
}
