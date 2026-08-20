using HarmonyLib;
using Rift;

namespace DavidInnaRework.CardPatches;

[HarmonyPatch(typeof(CardData), nameof(CardData.GetDescription))]
public static class BottledEctoplasmTriggersCursePatch
{
    private const int BottledEctoplasmCardId = 1432;

    static void Prefix(CardData __instance)
    {
        if (__instance == null || __instance._CardID != BottledEctoplasmCardId) return;

        foreach (var existingEffect in __instance._Effects)
        {
            if (existingEffect._Mode == EffectMode.TriggerEffect
                && existingEffect._AppliedEffect == AppliedEffectType.Curse)
            {
                return;
            }
        }

        var triggerCurseEffect = new CardEffect
        {
            CardData = __instance,
            _Mode = EffectMode.TriggerEffect,
            _AppliedEffect = AppliedEffectType.Curse,
            _Targeting = EffectTargeting.Ranged,
            _EffectValue = 1,
            _EffectValueUpgraded = 1,
        };

        __instance._Effects.Add(triggerCurseEffect);
    }
}

[HarmonyPatch(typeof(CardData), nameof(CardData.GetDescription))]
public static class BottledEctoplasmDescriptionPatch
{
    private const int BottledEctoplasmCardId = 1432;

    static void Prefix(CardData __instance)
    {
        if (__instance == null || __instance._CardID != BottledEctoplasmCardId) return;

        __instance._BaseDescription = "Give Curse ({0}) to any enemy, then trigger it {1} time.";
    }
}
