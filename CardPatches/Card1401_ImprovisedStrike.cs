using HarmonyLib;
using Rift;

namespace DavidInnaRework.CardPatches;

// Card 1401 "Improvised Strike" is rebuilt as:
//   - Deal 2x2 damage to the first enemy (2x3 upgraded).
//   - Draw one Improvised Strike when a Tool is played.
//
// The draw trigger is provided by the generic NoFatigueDrawOnToolPlayed
// mechanic, configured for this card in Plugin.Load().
[HarmonyPatch(typeof(CardData), nameof(CardData.GetDescription))]
public static class ImprovisedStrikeEffectPatch
{
    private const int ImprovisedStrikeCardId = 1401;
    private const int DamageValue = 2;
    private const int DamageValueUpgraded = 2;
    private const int DamageCount = 2;
    private const int DamageCountUpgraded = 3;

    static void Prefix(CardData __instance)
    {
        if (__instance == null || __instance._CardID != ImprovisedStrikeCardId) return;

        foreach (var effect in __instance._Effects)
        {
            if (effect._Mode == EffectMode.Damage
                && effect._EffectValue == DamageValue
                && effect._EffectValueUpgraded == DamageValueUpgraded
                && effect._EffectCount == DamageCount
                && effect._EffectCountUpgraded == DamageCountUpgraded)
            {
                return;
            }
        }

        __instance._Effects.Clear();
        __instance._Effects.Add(new CardEffect
        {
            CardData = __instance,
            _Mode = EffectMode.Damage,
            _Targeting = EffectTargeting.Melee,
            _EffectValue = DamageValue,
            _EffectValueUpgraded = DamageValueUpgraded,
            _EffectCount = DamageCount,
            _EffectCountUpgraded = DamageCountUpgraded,
        });
    }
}

[HarmonyPatch(typeof(CardData), nameof(CardData.GetDescription))]
public static class ImprovisedStrikeDescriptionPatch
{
    private const int ImprovisedStrikeCardId = 1401;
    private const string NewDescription =
        "Deal {0} damage to the first enemy. Draw one Improvised Strike when you play a Tool.";

    static void Prefix(CardData __instance)
    {
        if (__instance == null || __instance._CardID != ImprovisedStrikeCardId) return;

        __instance._BaseDescription = NewDescription;
    }
}
